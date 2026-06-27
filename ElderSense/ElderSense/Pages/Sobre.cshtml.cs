using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages
{
    /// <summary>
    /// Página de identificação do trabalho (curso, disciplina, autores,
    /// bibliotecas/frameworks usadas, e credenciais de teste), exigida
    /// pelas regras de avaliação do trabalho.
    /// </summary>
    public class SobreModel : PageModel
    {
        /// <summary>
        /// Carrega a página de identificação do trabalho
        /// </summary>
        public void OnGet()
        {
        }
    }
}