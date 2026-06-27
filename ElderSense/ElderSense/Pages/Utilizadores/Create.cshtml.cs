using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages.Utilizadores
{
    /// <summary>
    /// Página de criação de um novo utilizador
    /// </summary>
    [Authorize]
    public class CreateModel : PageModel
    {
        /// <summary>
        /// Gestor de utilizadores do Identity, usado para criar a conta com a password encriptada
        /// </summary>
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// Construtor que recebe o gestor de utilizadores injetado pelo sistema
        /// </summary>
        public CreateModel(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Utilizador submetido pelo formulário de criação
        /// </summary>
        [BindProperty]
        public Utilizador Utilizador { get; set; } = new();

        /// <summary>
        /// Password escolhida para a nova conta
        /// </summary>
        [BindProperty]
        public string Password { get; set; } = "";

        /// <summary>
        /// Carrega a página de criação
        /// </summary>
        public void OnGet() { }

        /// <summary>
        /// Processa a criação do utilizador através do UserManager
        /// </summary>
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