using FoodprintApi.Models.Domain;

namespace FoodprintApi.Services;

/// <summary>
/// Service for calculating carbon footprints of food ingredients
/// </summary>
public class CarbonCalculatorService : ICarbonCalculatorService
{
    private readonly ILogger<CarbonCalculatorService> _logger;
    
    // Carbon footprint data (kg CO2 per kg of ingredient)
    // Source: Various studies and environmental databases
    private static readonly Dictionary<string, decimal> CarbonFootprintData = new(StringComparer.OrdinalIgnoreCase)
    {
        // Proteins
        {"beef", 27.0m},
        {"lamb", 24.5m},
        {"pork", 7.6m},
        {"chicken", 6.1m},
        {"turkey", 5.8m},
        {"fish", 5.4m},
        {"salmon", 6.0m},
        {"tuna", 6.1m},
        {"shrimp", 11.8m},
        {"eggs", 4.2m},
        {"tofu", 2.0m},
        {"beans", 0.4m},
        {"lentils", 0.9m},
        {"chickpeas", 0.4m},
        
        // Grains and Starches
        {"rice", 2.7m},
        {"wheat", 1.4m},
        {"bread", 1.3m},
        {"pasta", 1.1m},
        {"potatoes", 0.3m},
        {"oats", 1.6m},
        {"quinoa", 1.8m},
        {"corn", 1.1m},
        {"barley", 1.2m},
        
        // Dairy
        {"milk", 3.2m},
        {"cheese", 13.5m},
        {"butter", 23.8m},
        {"yogurt", 2.2m},
        {"cream", 7.4m},
        
        // Vegetables
        {"tomatoes", 1.4m},
        {"onions", 0.3m},
        {"carrots", 0.4m},
        {"broccoli", 0.4m},
        {"spinach", 0.4m},
        {"lettuce", 0.5m},
        {"cabbage", 0.3m},
        {"peppers", 0.7m},
        {"cucumber", 0.5m},
        {"garlic", 0.6m},
        {"ginger", 0.8m},
        
        // Oils and Fats
        {"olive oil", 6.3m},
        {"vegetable oil", 6.0m},
        {"coconut oil", 6.4m},
        {"sunflower oil", 5.8m},
        {"palm oil", 7.6m},
        
        // Spices and Others
        {"salt", 0.04m},
        {"sugar", 1.8m},
        {"black pepper", 7.0m},
        {"turmeric", 3.0m},
        {"cumin", 4.2m},
        {"coriander", 3.8m},
        {"cinnamon", 5.5m},
        {"cardamom", 8.2m},
        
        // Nuts and Seeds
        {"almonds", 8.8m},
        {"cashews", 7.9m},
        {"peanuts", 2.5m},
        {"walnuts", 7.2m},
        {"sesame seeds", 5.9m},
        
        // Fruits
        {"apples", 0.4m},
        {"bananas", 0.7m},
        {"oranges", 0.4m},
        {"lemons", 0.6m},
        {"coconut", 1.7m}
    };

    public CarbonCalculatorService(ILogger<CarbonCalculatorService> logger)
    {
        _logger = logger;
    }

    public async Task<List<Ingredient>> CalculateCarbonFootprintAsync(List<Ingredient> ingredients)
    {
        _logger.LogInformation("Calculating carbon footprint for {Count} ingredients", ingredients.Count);
        
        var tasks = ingredients.Select(async ingredient =>
        {
            var carbonPerKg = await GetIngredientCarbonFootprintAsync(ingredient.Name, ingredient.Category);
            ingredient.CarbonPerKg = carbonPerKg;
            return ingredient;
        });

        var results = await Task.WhenAll(tasks);
        
        _logger.LogInformation("Carbon footprint calculation completed. Total CO2: {TotalCarbon:F2} kg", 
            results.Sum(i => i.TotalCarbonKg));
        
        return results.ToList();
    }

    public async Task<decimal> GetIngredientCarbonFootprintAsync(string ingredientName, string category)
    {
        await Task.Delay(1); // Simulate async operation
        
        // Try exact match first
        if (CarbonFootprintData.TryGetValue(ingredientName, out var exactMatch))
        {
            _logger.LogDebug("Found exact match for {Ingredient}: {Carbon:F2} kg CO2/kg", ingredientName, exactMatch);
            return exactMatch;
        }

        // Try partial matches
        var partialMatch = CarbonFootprintData.Keys
            .Where(key => ingredientName.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                         key.Contains(ingredientName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (partialMatch != null)
        {
            var carbon = CarbonFootprintData[partialMatch];
            _logger.LogDebug("Found partial match for {Ingredient} -> {Match}: {Carbon:F2} kg CO2/kg", 
                ingredientName, partialMatch, carbon);
            return carbon;
        }

        // Fallback to category averages
        var categoryAverage = GetCategoryAverage(category);
        _logger.LogDebug("Using category average for {Ingredient} ({Category}): {Carbon:F2} kg CO2/kg", 
            ingredientName, category, categoryAverage);
        
        return categoryAverage;
    }

    private static decimal GetCategoryAverage(string category)
    {
        return category.ToLower() switch
        {
            "protein" => 8.5m, // Average of various proteins
            "grain" => 1.5m,   // Average of grains
            "vegetable" => 0.5m, // Average of vegetables
            "dairy" => 8.0m,   // Average of dairy products
            "oil" => 6.2m,     // Average of oils
            "spice" => 4.0m,   // Average of spices
            "other" => 2.0m,   // General average
            _ => 2.0m          // Default fallback
        };
    }
}