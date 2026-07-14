using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ElderSense.Swagger
{
    /// <summary>
    /// Filtro que aplica o requisito de segurança JWT ao documento OpenAPI inteiro.
    /// A referência recebe o próprio documento (em vez de null) para se conseguir
    /// resolver e serializar corretamente no swagger.json
    /// </summary>
    public class SecurityDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Adiciona o requisito de segurança Bearer ao documento OpenAPI gerado
        /// </summary>
        public void Apply(OpenApiDocument doc, DocumentFilterContext context)
        {
            doc.Security ??= new List<OpenApiSecurityRequirement>();
            doc.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", doc),
                    new List<string>()
                }
            });
        }
    }
}