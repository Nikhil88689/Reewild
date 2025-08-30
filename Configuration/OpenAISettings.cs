namespace FoodprintApi.Configuration;

/// <summary>
/// Configuration settings for OpenAI API
/// </summary>
public class OpenAISettings
{
    /// <summary>
    /// OpenAI API Key
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Model to use for text-based analysis
    /// </summary>
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    /// Model to use for vision-based analysis
    /// </summary>
    public string VisionModel { get; set; } = "gpt-4-vision-preview";

    /// <summary>
    /// Maximum tokens for response
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Temperature for response generation
    /// </summary>
    public double Temperature { get; set; } = 0.3;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}