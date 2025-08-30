namespace FoodprintApi.Models.Responses;

/// <summary>
/// Standard error response model
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error code
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Additional details about the error
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request ID for tracking
    /// </summary>
    public string? RequestId { get; set; }
}

/// <summary>
/// Validation error response
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Field-specific validation errors
    /// </summary>
    public Dictionary<string, List<string>>? ValidationErrors { get; set; }
}