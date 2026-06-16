using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages.Utilizadores
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;

        public CreateModel(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public Utilizador Utilizador { get; set; } = new();

        [BindProperty]
        public string Password { get; set; } = "";

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Utilizador.PasswordHash");

            if (!ModelState.IsValid) return Page();

            // define o username igual ao email
            Utilizador.UserName = Utilizador.Email;

            // cria o utilizador com o UserManager para garantir que a password é encriptada
            var result = await _userManager.CreateAsync(Utilizador, Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}