namespace FoodprintApi.Models.Responses;

/// <summary>
/// Response model for carbon footprint estimation
/// </summary>
public class CarbonFootprintResponse
{
    /// <summary>
    /// Name of the analyzed dish
    /// </summary>
    public required string Dish { get; set; }

    /// <summary>
    /// Total estimated carbon footprint in kg CO2
    /// </summary>
    public decimal EstimatedCarbonKg { get; set; }

    /// <summary>
    /// Breakdown of ingredients and their carbon footprints
    /// </summary>
    public required List<IngredientCarbon> Ingredients { get; set; }

    /// <summary>
    /// Confidence level of the estimation (0.0 to 1.0)
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Additional metadata about the analysis
    /// </summary>
    public AnalysisMetadata? Metadata { get; set; }
}

/// <summary>
/// Individual ingredient carbon footprint
/// </summary>
public class IngredientCarbon
{
    /// <summary>
    /// Name of the ingredient
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Estimated carbon footprint for this ingredient in kg CO2
    /// </summary>
    public decimal CarbonKg { get; set; }

    /// <summary>
    /// Estimated quantity/portion size
    /// </summary>
    public string? EstimatedQuantity { get; set; }

    /// <summary>
    /// Category of the ingredient (protein, grain, vegetable, etc.)
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// Metadata about the analysis process
/// </summary>
public class AnalysisMetadata
{
    /// <summary>
    /// Method used for analysis (text, image)
    /// </summary>
    public required string AnalysisMethod { get; set; }

    /// <summary>
    /// Timestamp of the analysis
    /// </summary>
    public DateTime AnalyzedAt { get; set; }

    /// <summary>
    /// Model used for ingredient extraction
    /// </summary>
    public string? ModelUsed { get; set; }

    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}