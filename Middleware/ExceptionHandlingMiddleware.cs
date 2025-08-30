using FoodprintApi.Models.Responses;
using System.Net;
using System.Text.Json;

namespace FoodprintApi.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;
        
        _logger.LogError(exception, "Unhandled exception occurred (RequestId: {RequestId})", requestId);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            ArgumentException or ArgumentNullException => new ErrorResponse
            {
                Code = "BAD_REQUEST",
                Message = exception.Message,
                RequestId = requestId
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                Code = "UNAUTHORIZED",
                Message = "Access denied",
                RequestId = requestId
            },
            TimeoutException or TaskCanceledException => new ErrorResponse
            {
                Code = "TIMEOUT",
                Message = "The operation timed out",
                RequestId = requestId
            },
            _ => new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred",
                Details = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true 
                    ? exception.Message 
                    : null,
                RequestId = requestId
            }
        };

        response.StatusCode = exception switch
        {
            ArgumentException or ArgumentNullException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            TimeoutException or TaskCanceledException => (int)HttpStatusCode.RequestTimeout,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}