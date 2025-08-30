using FoodprintApi.Models.Domain;

namespace FoodprintApi.Services;

/// <summary>
/// Service for LLM-based ingredient extraction from text
/// </summary>
public interface ILLMService
{
    /// <summary>
    /// Analyzes a dish name and extracts ingredients with estimated quantities
    /// </summary>
    /// <param name="dishName">Name of the dish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis result with ingredients</returns>
    Task<DishAnalysis> AnalyzeDishAsync(string dishName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for vision-based ingredient extraction from images
/// </summary>
public interface IVisionService
{
    /// <summary>
    /// Analyzes an image and extracts dish information and ingredients
    /// </summary>
    /// <param name="imageStream">Image stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis result with ingredients</returns>
    Task<DishAnalysis> AnalyzeImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for calculating carbon footprints of ingredients
/// </summary>
public interface ICarbonCalculatorService
{
    /// <summary>
    /// Calculates carbon footprint for ingredients
    /// </summary>
    /// <param name="ingredients">List of ingredients</param>
    /// <returns>Updated ingredients with carbon calculations</returns>
    Task<List<Ingredient>> CalculateCarbonFootprintAsync(List<Ingredient> ingredients);

    /// <summary>
    /// Gets carbon footprint data for a specific ingredient
    /// </summary>
    /// <param name="ingredientName">Name of the ingredient</param>
    /// <param name="category">Category of the ingredient</param>
    /// <returns>Carbon footprint per kg</returns>
    Task<decimal> GetIngredientCarbonFootprintAsync(string ingredientName, string category);
}