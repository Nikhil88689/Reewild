using FoodprintApi.Models.Requests;
using FoodprintApi.Models.Responses;
using FoodprintApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FoodprintApi.Controllers;

/// <summary>
/// Controller for carbon footprint estimation endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EstimateController : ControllerBase
{
    private readonly ILLMService _llmService;
    private readonly IVisionService _visionService;
    private readonly ICarbonCalculatorService _carbonCalculatorService;
    private readonly ILogger<EstimateController> _logger;

    public EstimateController(
        ILLMService llmService,
        IVisionService visionService,
        ICarbonCalculatorService carbonCalculatorService,
        ILogger<EstimateController> logger)
    {
        _llmService = llmService;
        _visionService = visionService;
        _carbonCalculatorService = carbonCalculatorService;
        _logger = logger;
    }

    /// <summary>
    /// Estimates carbon footprint for a dish based on its name
    /// </summary>
    /// <param name="request">Dish analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Carbon footprint estimation</returns>
    [HttpPost]
    [ProducesResponseType<CarbonFootprintResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CarbonFootprintResponse>> EstimateDish(
        [FromBody] EstimateDishRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = HttpContext.TraceIdentifier;

        try
        {
            _logger.LogInformation("Processing dish estimation request for: {Dish} (RequestId: {RequestId})", 
                request.Dish, requestId);

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                return BadRequest(new ValidationErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Invalid request data",
                    ValidationErrors = validationErrors,
                    RequestId = requestId
                });
            }

            // Analyze dish using LLM
            var dishAnalysis = await _llmService.AnalyzeDishAsync(request.Dish, cancellationToken);

            // Calculate carbon footprints
            var ingredientsWithCarbon = await _carbonCalculatorService.CalculateCarbonFootprintAsync(dishAnalysis.Ingredients);

            stopwatch.Stop();

            // Build response
            var response = new CarbonFootprintResponse
            {
                Dish = dishAnalysis.DishName,
                EstimatedCarbonKg = ingredientsWithCarbon.Sum(i => i.TotalCarbonKg),
                Confidence = dishAnalysis.OverallConfidence,
                Ingredients = ingredientsWithCarbon.Select(i => new IngredientCarbon
                {
                    Name = i.Name,
                    CarbonKg = i.TotalCarbonKg,
                    EstimatedQuantity = $"{i.EstimatedQuantityGrams:F0}g",
                    Category = i.Category
                }).ToList(),
                Metadata = new AnalysisMetadata
                {
                    AnalysisMethod = dishAnalysis.AnalysisMethod,
                    AnalyzedAt = DateTime.UtcNow,
                    ModelUsed = "gpt-4",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                }
            };

            _logger.LogInformation("Dish estimation completed for: {Dish}. Carbon: {Carbon:F2} kg CO2 (RequestId: {RequestId})", 
                request.Dish, response.EstimatedCarbonKg, requestId);

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Dish estimation request was cancelled (RequestId: {RequestId})", requestId);
            return StatusCode(StatusCodes.Status499ClientClosedRequest, new ErrorResponse
            {
                Code = "REQUEST_CANCELLED",
                Message = "Request was cancelled",
                RequestId = requestId
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing dish estimation for: {Dish} (RequestId: {RequestId})", 
                request.Dish, requestId);

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while processing your request",
                Details = HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true 
                    ? ex.Message 
                    : null,
                RequestId = requestId
            });
        }
    }

    /// <summary>
    /// Estimates carbon footprint for a dish based on an uploaded image
    /// </summary>
    /// <param name="image">Image file to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Carbon footprint estimation</returns>
    [HttpPost("image")]
    [ProducesResponseType<CarbonFootprintResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    [RequestSizeLimit(20_971_520)] // 20MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 20_971_520)]
    [ApiExplorerSettings(IgnoreApi = true)] // Temporarily hide from Swagger
    public async Task<ActionResult<CarbonFootprintResponse>> EstimateImage(
        IFormFile image,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = HttpContext.TraceIdentifier;

        try
        {
            _logger.LogInformation("Processing image estimation request for: {FileName} (RequestId: {RequestId})", 
                image?.FileName, requestId);

            // Validate image
            if (image == null || image.Length == 0)
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Image file is required",
                    ValidationErrors = new Dictionary<string, List<string>>
                    {
                        { "image", new List<string> { "Image file is required and cannot be empty" } }
                    },
                    RequestId = requestId
                });
            }

            // Additional validation
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(image.ContentType?.ToLower()))
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Invalid image format",
                    ValidationErrors = new Dictionary<string, List<string>>
                    {
                        { "image", new List<string> { $"Unsupported image format: {image.ContentType}. Allowed formats: JPEG, PNG, GIF, WebP" } }
                    },
                    RequestId = requestId
                });
            }

            if (image.Length > 20 * 1024 * 1024) // 20MB
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Image file too large",
                    ValidationErrors = new Dictionary<string, List<string>>
                    {
                        { "image", new List<string> { "Image file must be smaller than 20MB" } }
                    },
                    RequestId = requestId
                });
            }

            // Analyze image using Vision service
            using var imageStream = image.OpenReadStream();
            var dishAnalysis = await _visionService.AnalyzeImageAsync(imageStream, image.FileName!, cancellationToken);

            // Calculate carbon footprints
            var ingredientsWithCarbon = await _carbonCalculatorService.CalculateCarbonFootprintAsync(dishAnalysis.Ingredients);

            stopwatch.Stop();

            // Build response
            var response = new CarbonFootprintResponse
            {
                Dish = dishAnalysis.DishName,
                EstimatedCarbonKg = ingredientsWithCarbon.Sum(i => i.TotalCarbonKg),
                Confidence = dishAnalysis.OverallConfidence,
                Ingredients = ingredientsWithCarbon.Select(i => new IngredientCarbon
                {
                    Name = i.Name,
                    CarbonKg = i.TotalCarbonKg,
                    EstimatedQuantity = $"{i.EstimatedQuantityGrams:F0}g",
                    Category = i.Category
                }).ToList(),
                Metadata = new AnalysisMetadata
                {
                    AnalysisMethod = dishAnalysis.AnalysisMethod,
                    AnalyzedAt = DateTime.UtcNow,
                    ModelUsed = "gpt-4-vision-preview",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                }
            };

            _logger.LogInformation("Image estimation completed for: {FileName}. Carbon: {Carbon:F2} kg CO2 (RequestId: {RequestId})", 
                image.FileName, response.EstimatedCarbonKg, requestId);

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Image estimation request was cancelled (RequestId: {RequestId})", requestId);
            return StatusCode(StatusCodes.Status499ClientClosedRequest, new ErrorResponse
            {
                Code = "REQUEST_CANCELLED",
                Message = "Request was cancelled",
                RequestId = requestId
            });
        }
        catch (ArgumentException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Invalid image input (RequestId: {RequestId})", requestId);

            return BadRequest(new ValidationErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = ex.Message,
                RequestId = requestId
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing image estimation for: {FileName} (RequestId: {RequestId})", 
                image?.FileName, requestId);

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while processing your request",
                Details = HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true 
                    ? ex.Message 
                    : null,
                RequestId = requestId
            });
        }
    }
}