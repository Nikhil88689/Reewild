using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace FoodprintApi.Configuration;

/// <summary>
/// Swagger operation filter to handle file upload parameters
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToList();

        if (!fileParameters.Any())
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParameters.ToDictionary(
                            p => p.Name!,
                            p => new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        ),
                        Required = fileParameters.Select(p => p.Name!).ToHashSet()
                    }
                }
            }
        };

        // Remove any existing parameters that are file uploads
        foreach (var fileParam in fileParameters)
        {
            var paramToRemove = operation.Parameters?
                .FirstOrDefault(p => p.Name == fileParam.Name);
            if (paramToRemove != null && operation.Parameters != null)
            {
                operation.Parameters.Remove(paramToRemove);
            }
        }
    }
}