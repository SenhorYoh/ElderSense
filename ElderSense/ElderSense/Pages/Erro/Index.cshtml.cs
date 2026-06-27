using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages.Erro
{
    /// <summary>
    /// Página de erro personalizada, acionada pelo UseStatusCodePagesWithReExecute
    /// para mostrar uma mensagem contextual de acordo com o código HTTP recebido (404, 403, 500, etc.)
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>
        /// Código de erro recebido na rota, ex: 404, 403
        /// </summary>
        public int CodigoErro { get; set; }

        /// <summary>
        /// Mensagem contextual a mostrar ao utilizador, de acordo com o código de erro
        /// </summary>
        public string Mensagem { get; set; } = "";

        /// <summary>
        /// Carrega a página de erro e define a mensagem correspondente ao código recebido
        /// </summary>
        public void OnGet(int codigo)
        {
            CodigoErro = codigo;

            Mensagem = codigo switch
            {
                404 => "A página que procuras não existe ou foi removida.",
                403 => "Não tens permissão para aceder a esta página.",
                500 => "Ocorreu um erro interno no servidor.",
                _ => "Ocorreu um erro inesperado."
            };
        }
    }
}