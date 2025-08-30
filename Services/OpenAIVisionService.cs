using FoodprintApi.Configuration;
using FoodprintApi.Models.Domain;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

namespace FoodprintApi.Services;

/// <summary>
/// OpenAI Vision-based service for image analysis
/// </summary>
public class OpenAIVisionService : IVisionService
{
    private readonly ChatClient _chatClient;
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIVisionService> _logger;

    public OpenAIVisionService(IOptions<OpenAISettings> settings, ILogger<OpenAIVisionService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _chatClient = new ChatClient(_settings.VisionModel, _settings.ApiKey);
    }

    public async Task<DishAnalysis> AnalyzeImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing image: {FileName}", fileName);

            // Validate image
            ValidateImage(imageStream, fileName);

            // Convert image to base64
            var imageBytes = await ReadStreamAsync(imageStream);
            var base64Image = Convert.ToBase64String(imageBytes);
            var mimeType = GetMimeType(fileName);

            var prompt = CreateVisionAnalysisPrompt(base64Image, mimeType);
            
            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _settings.MaxTokens,
                Temperature = (float)_settings.Temperature
            };

            var response = await _chatClient.CompleteChatAsync(prompt, chatOptions, cancellationToken);
            
            var content = response.Value.Content.FirstOrDefault()?.Text;
            
            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("Empty response from OpenAI Vision");
            }

            _logger.LogDebug("OpenAI Vision response: {Response}", content);

            return ParseVisionResponse(content, fileName, "image");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image {FileName}", fileName);
            throw;
        }
    }

    private static void ValidateImage(Stream imageStream, string fileName)
    {
        if (imageStream.Length == 0)
        {
            throw new ArgumentException("Image stream is empty");
        }

        if (imageStream.Length > 20 * 1024 * 1024) // 20MB limit
        {
            throw new ArgumentException("Image file too large (max 20MB)");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(fileName).ToLower();
        
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Unsupported image format: {extension}");
        }
    }

    private static async Task<byte[]> ReadStreamAsync(Stream stream)
    {
        stream.Position = 0;
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    private static List<ChatMessage> CreateVisionAnalysisPrompt(string base64Image, string mimeType)
    {
        var systemPrompt = @"You are a food analysis expert. Analyze the food image and identify the dish and its likely ingredients with estimated quantities.

Return your response in the following JSON format:
{
  ""dishName"": ""identified dish name"",
  ""ingredients"": [
    {
      ""name"": ""ingredient name"",
      ""category"": ""protein|grain|vegetable|dairy|oil|spice|other"",
      ""estimatedQuantityGrams"": 100,
      ""confidence"": 0.85
    }
  ],
  ""overallConfidence"": 0.80
}

Guidelines:
- Identify the main dish first
- Be realistic about portion sizes visible in the image
- Categories: protein, grain, vegetable, dairy, oil, spice, other
- Confidence should be between 0.0 and 1.0
- Include visible ingredients only
- Consider typical preparation methods for the identified dish";

        var userMessage = new UserChatMessage(
            new ChatMessageContentPart[]
            {
                ChatMessageContentPart.CreateTextPart("Analyze this food image and identify the dish and its ingredients:"),
                ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(Convert.FromBase64String(base64Image)), mimeType)
            }
        );

        return new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            userMessage
        };
    }

    private DishAnalysis ParseVisionResponse(string content, string fileName, string method)
    {
        try
        {
            // Clean up the response to extract JSON
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart == -1 || jsonEnd == -1)
            {
                throw new InvalidOperationException("No valid JSON found in response");
            }

            var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = JsonSerializer.Deserialize<VisionResponseDto>(jsonContent, options);
            
            if (response?.Ingredients == null)
            {
                throw new InvalidOperationException("Invalid response format");
            }

            var ingredients = response.Ingredients.Select(i => new Ingredient
            {
                Name = i.Name ?? "Unknown",
                Category = i.Category ?? "other",
                EstimatedQuantityGrams = i.EstimatedQuantityGrams,
                Confidence = i.Confidence,
                CarbonPerKg = 0 // Will be calculated by carbon service
            }).ToList();

            return new DishAnalysis
            {
                DishName = response.DishName ?? Path.GetFileNameWithoutExtension(fileName),
                Ingredients = ingredients,
                OverallConfidence = response.OverallConfidence,
                AnalysisMethod = method
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing vision response: {Content}", content);
            
            // Fallback: create a basic analysis
            return CreateFallbackAnalysis(fileName, method);
        }
    }

    private static DishAnalysis CreateFallbackAnalysis(string fileName, string method)
    {
        var dishName = Path.GetFileNameWithoutExtension(fileName);
        
        var ingredients = new List<Ingredient>
        {
            new() { Name = "Mixed ingredients", Category = "other", EstimatedQuantityGrams = 250, Confidence = 0.3m }
        };

        return new DishAnalysis
        {
            DishName = dishName,
            Ingredients = ingredients,
            OverallConfidence = 0.3m,
            AnalysisMethod = method
        };
    }

    private class VisionResponseDto
    {
        public string? DishName { get; set; }
        public List<IngredientDto>? Ingredients { get; set; }
        public decimal OverallConfidence { get; set; }
    }

    private class IngredientDto
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public decimal EstimatedQuantityGrams { get; set; }
        public decimal Confidence { get; set; }
    }
}