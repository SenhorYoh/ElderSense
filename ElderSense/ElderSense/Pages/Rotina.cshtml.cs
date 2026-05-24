using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages
{
    // Apenas utilizadores autenticados podem aceder à rotina
    [Authorize]
    public class RotinaModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
