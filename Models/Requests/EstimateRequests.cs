using System.ComponentModel.DataAnnotations;

namespace FoodprintApi.Models.Requests;

/// <summary>
/// Request model for dish-based carbon footprint estimation
/// </summary>
public class EstimateDishRequest
{
    /// <summary>
    /// Name of the dish to analyze
    /// </summary>
    [Required(ErrorMessage = "Dish name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Dish name must be between 2 and 200 characters")]
    public required string Dish { get; set; }
}

/// <summary>
/// Request model for image-based carbon footprint estimation
/// </summary>
public class EstimateImageRequest
{
    /// <summary>
    /// Image file for analysis
    /// </summary>
    [Required(ErrorMessage = "Image file is required")]
    public required IFormFile Image { get; set; }
}