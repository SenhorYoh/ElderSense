using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages.Shared
{
    /// <summary>
    /// PageModel do partial _AvisoSemIdoso, que mostra um aviso quando o Cuidador
    /// autenticado ainda não tem nenhum Idoso associado à sua conta
    /// </summary>
    public class _AvisoSemIdosoModel : PageModel
    {
        /// <summary>
        /// Não é usado diretamente, já que este ficheiro é renderizado como partial
        /// </summary>
        public void OnGet()
        {
        }
    }
}