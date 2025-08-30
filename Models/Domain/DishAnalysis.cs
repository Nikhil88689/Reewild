namespace FoodprintApi.Models.Domain;

/// <summary>
/// Domain model representing a food ingredient
/// </summary>
public class Ingredient
{
    /// <summary>
    /// Name of the ingredient
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Category of the ingredient
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Carbon footprint per unit (kg CO2 per kg of ingredient)
    /// </summary>
    public decimal CarbonPerKg { get; set; }

    /// <summary>
    /// Estimated quantity in the dish (in grams)
    /// </summary>
    public decimal EstimatedQuantityGrams { get; set; }

    /// <summary>
    /// Total carbon footprint for this ingredient in the dish
    /// </summary>
    public decimal TotalCarbonKg => (EstimatedQuantityGrams / 1000) * CarbonPerKg;

    /// <summary>
    /// Confidence level for this ingredient identification
    /// </summary>
    public decimal Confidence { get; set; }
}

/// <summary>
/// Domain model for a dish analysis result
/// </summary>
public class DishAnalysis
{
    /// <summary>
    /// Name of the dish
    /// </summary>
    public required string DishName { get; set; }

    /// <summary>
    /// List of identified ingredients
    /// </summary>
    public required List<Ingredient> Ingredients { get; set; }

    /// <summary>
    /// Overall confidence in the analysis
    /// </summary>
    public decimal OverallConfidence { get; set; }

    /// <summary>
    /// Analysis method used
    /// </summary>
    public required string AnalysisMethod { get; set; }

    /// <summary>
    /// Total carbon footprint
    /// </summary>
    public decimal TotalCarbonKg => Ingredients.Sum(i => i.TotalCarbonKg);
}