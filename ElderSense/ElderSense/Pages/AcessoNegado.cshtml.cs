using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages
{
    /// <summary>
    /// Página mostrada quando um utilizador autenticado tenta aceder a uma
    /// área para a qual não tem permissão (ex: rota exclusiva de outra role)
    /// </summary>
    public class AcessoNegadoModel : PageModel
    {
        /// <summary>
        /// Carrega a página de acesso negado
        /// </summary>
        public void OnGet()
        {
        }
    }
}