using FoodprintApi.Configuration;
using FoodprintApi.Models.Domain;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

namespace FoodprintApi.Services;

/// <summary>
/// OpenAI-based LLM service for ingredient extraction
/// </summary>
public class OpenAIService : ILLMService
{
    private readonly ChatClient _chatClient;
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IOptions<OpenAISettings> settings, ILogger<OpenAIService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _chatClient = new ChatClient(_settings.Model, _settings.ApiKey);
    }

    public async Task<DishAnalysis> AnalyzeDishAsync(string dishName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing dish: {DishName}", dishName);

            var prompt = CreateDishAnalysisPrompt(dishName);
            
            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _settings.MaxTokens,
                Temperature = (float)_settings.Temperature
            };

            var response = await _chatClient.CompleteChatAsync(prompt, chatOptions, cancellationToken);
            
            var content = response.Value.Content.FirstOrDefault()?.Text;
            
            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("Empty response from OpenAI");
            }

            _logger.LogDebug("OpenAI response: {Response}", content);

            return ParseAnalysisResponse(content, dishName, "text");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing dish {DishName}, using fallback analysis", dishName);
            
            // Return fallback analysis instead of throwing
            return CreateFallbackAnalysis(dishName, "text");
        }
    }

    private static List<ChatMessage> CreateDishAnalysisPrompt(string dishName)
    {
        var systemPrompt = @"You are a food analysis expert. Given a dish name, identify the likely ingredients and estimate their quantities.

Return your response in the following JSON format:
{
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
- Be realistic about portion sizes for a typical serving
- Categories: protein, grain, vegetable, dairy, oil, spice, other
- Confidence should be between 0.0 and 1.0
- Include main ingredients only (skip water, salt unless significant)
- Consider cultural context and typical preparation methods";

        var userPrompt = $"Analyze this dish and identify its ingredients: {dishName}";

        return new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };
    }

    private DishAnalysis ParseAnalysisResponse(string content, string dishName, string method)
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

            var response = JsonSerializer.Deserialize<AnalysisResponseDto>(jsonContent, options);
            
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
                DishName = dishName,
                Ingredients = ingredients,
                OverallConfidence = response.OverallConfidence,
                AnalysisMethod = method
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing analysis response: {Content}", content);
            
            // Fallback: create a basic analysis
            return CreateFallbackAnalysis(dishName, method);
        }
    }

    private static DishAnalysis CreateFallbackAnalysis(string dishName, string method)
    {
        // Enhanced fallback based on dish name keywords
        var ingredients = new List<Ingredient>();
        var dishLower = dishName.ToLower();
        
        // Chicken dishes
        if (dishLower.Contains("chicken"))
        {
            ingredients.Add(new Ingredient { Name = "Chicken", Category = "protein", EstimatedQuantityGrams = 150, Confidence = 0.8m });
            
            if (dishLower.Contains("biryani"))
            {
                ingredients.Add(new Ingredient { Name = "Basmati Rice", Category = "grain", EstimatedQuantityGrams = 200, Confidence = 0.9m });
                ingredients.Add(new Ingredient { Name = "Onions", Category = "vegetable", EstimatedQuantityGrams = 50, Confidence = 0.7m });
                ingredients.Add(new Ingredient { Name = "Spices", Category = "spice", EstimatedQuantityGrams = 10, Confidence = 0.8m });
                ingredients.Add(new Ingredient { Name = "Cooking Oil", Category = "oil", EstimatedQuantityGrams = 15, Confidence = 0.7m });
            }
        }
        // Beef dishes
        else if (dishLower.Contains("beef"))
        {
            ingredients.Add(new Ingredient { Name = "Beef", Category = "protein", EstimatedQuantityGrams = 150, Confidence = 0.8m });
        }
        // Pizza
        else if (dishLower.Contains("pizza"))
        {
            ingredients.Add(new Ingredient { Name = "Wheat Flour", Category = "grain", EstimatedQuantityGrams = 100, Confidence = 0.8m });
            ingredients.Add(new Ingredient { Name = "Cheese", Category = "dairy", EstimatedQuantityGrams = 80, Confidence = 0.9m });
            ingredients.Add(new Ingredient { Name = "Tomato Sauce", Category = "vegetable", EstimatedQuantityGrams = 30, Confidence = 0.8m });
        }
        // Rice dishes
        else if (dishLower.Contains("rice") || dishLower.Contains("biryani"))
        {
            ingredients.Add(new Ingredient { Name = "Rice", Category = "grain", EstimatedQuantityGrams = 200, Confidence = 0.8m });
        }
        
        // Default fallback
        if (ingredients.Count == 0)
        {
            ingredients.Add(new Ingredient { Name = "Mixed ingredients", Category = "other", EstimatedQuantityGrams = 250, Confidence = 0.5m });
        }

        return new DishAnalysis
        {
            DishName = dishName,
            Ingredients = ingredients,
            OverallConfidence = 0.7m,
            AnalysisMethod = $"{method} (fallback)"
        };
    }

    private class AnalysisResponseDto
    {
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