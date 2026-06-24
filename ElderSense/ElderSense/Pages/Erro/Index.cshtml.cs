using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages.Erro
{
    public class IndexModel : PageModel
    {
        // o código de erro recebido na rota, ex: 404, 403
        public int CodigoErro { get; set; }

        public string Mensagem { get; set; } = "";

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