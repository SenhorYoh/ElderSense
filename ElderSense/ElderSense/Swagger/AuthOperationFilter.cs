using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ElderSense.Swagger
{
    /// <summary>
    /// Filtro que adiciona o requisito de segurança JWT a todas as operações da API no Swagger,
    /// fazendo com que o botão Authorize envie o token no cabeçalho
    /// </summary>
    public class AuthOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", null),
                        new List<string>()
                    }
                }
            };
        }
    }
}