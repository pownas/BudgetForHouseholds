using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BudgetApp.Api.DTOs
{
    public class AddFileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasFileUpload = context.ApiDescription.ParameterDescriptions
                .Any(p => p.ModelMetadata?.ContainerType == typeof(Microsoft.AspNetCore.Http.IFormFile));

            if (hasFileUpload)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties =
                                {
                                    ["file"] = new OpenApiSchema { Type = "string", Format = "binary" }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
