using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages.Shared
{
    /// <summary>
    /// PageModel do layout das páginas públicas (landing page, kits, sobre),
    /// sem sidebar nem navbar do Bootstrap, com navegação própria embutida em cada página
    /// </summary>
    public class _LayoutLandingModel : PageModel
    {
        /// <summary>
        /// Não é usado diretamente, já que este ficheiro é usado apenas como layout
        /// </summary>
        public void OnGet()
        {
        }
    }
}