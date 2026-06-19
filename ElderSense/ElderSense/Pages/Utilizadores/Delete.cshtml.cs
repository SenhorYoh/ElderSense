using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages.Utilizadores
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;

        public DeleteModel(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public Utilizador Utilizador { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // vai buscar o utilizador pelo id
            var utilizador = await _userManager.FindByIdAsync(id);

            if (utilizador == null) return NotFound();

            Utilizador = utilizador;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // vai buscar o utilizador pelo id para garantir que existe
            var utilizador = await _userManager.FindByIdAsync(Utilizador.Id);

            if (utilizador != null)
                await _userManager.DeleteAsync(utilizador);

            return RedirectToPage("Index");
        }
    }
}