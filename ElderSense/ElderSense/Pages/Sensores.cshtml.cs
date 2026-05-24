using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages
{
    // Apenas utilizadores autenticados podem aceder aos sensores
    [Authorize]
    public class SensoresModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
