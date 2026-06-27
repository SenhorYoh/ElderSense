using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace ElderSense.Pages
{
    /// <summary>
    /// Página de erro genérica, acionada pelo UseExceptionHandler quando ocorre
    /// uma exceção não tratada (diferente da página /Erro, que trata códigos de status HTTP)
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// Identificador único do pedido, usado para diagnóstico do erro
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indica se existe um RequestId válido a mostrar
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Carrega a página de erro e obtém o identificador do pedido atual
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}