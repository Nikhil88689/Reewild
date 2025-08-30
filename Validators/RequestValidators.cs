using FluentValidation;
using FoodprintApi.Models.Requests;

namespace FoodprintApi.Validators;

/// <summary>
/// Validator for EstimateDishRequest
/// </summary>
public class EstimateDishRequestValidator : AbstractValidator<EstimateDishRequest>
{
    public EstimateDishRequestValidator()
    {
        RuleFor(x => x.Dish)
            .NotEmpty()
            .WithMessage("Dish name is required")
            .Length(2, 200)
            .WithMessage("Dish name must be between 2 and 200 characters")
            .Must(BeValidDishName)
            .WithMessage("Dish name contains invalid characters");
    }

    private static bool BeValidDishName(string dishName)
    {
        if (string.IsNullOrWhiteSpace(dishName))
            return false;

        // Check for potentially malicious content
        var invalidPatterns = new[] { "<script", "javascript:", "data:", "vbscript:" };
        var dishLower = dishName.ToLower();
        
        return !invalidPatterns.Any(pattern => dishLower.Contains(pattern));
    }
}

/// <summary>
/// Validator for EstimateImageRequest
/// </summary>
public class EstimateImageRequestValidator : AbstractValidator<EstimateImageRequest>
{
    private static readonly string[] AllowedImageTypes = {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
    };

    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20MB

    public EstimateImageRequestValidator()
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Image file is required")
            .Must(BeValidImageFile)
            .WithMessage("Invalid image file")
            .Must(BeValidImageSize)
            .WithMessage($"Image file must be smaller than {MaxFileSizeBytes / (1024 * 1024)}MB")
            .Must(BeValidImageType)
            .WithMessage($"Image must be one of the following types: {string.Join(", ", AllowedImageTypes)}");
    }

    private static bool BeValidImageFile(IFormFile? file)
    {
        return file != null && file.Length > 0;
    }

    private static bool BeValidImageSize(IFormFile? file)
    {
        return file == null || file.Length <= MaxFileSizeBytes;
    }

    private static bool BeValidImageType(IFormFile? file)
    {
        return file == null || AllowedImageTypes.Contains(file.ContentType?.ToLower());
    }
}