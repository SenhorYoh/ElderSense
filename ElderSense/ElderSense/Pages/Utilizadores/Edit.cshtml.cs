using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ElderSense.Pages.Utilizadores
{
    /// <summary>
    /// Página de edição de um utilizador existente
    /// </summary>
    [Authorize]
    public class EditModel : PageModel
    {
        /// <summary>
        /// Gestor de utilizadores do Identity, usado para procurar e atualizar a conta
        /// </summary>
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// Construtor que recebe o gestor de utilizadores injetado pelo sistema
        /// </summary>
        public EditModel(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Utilizador a editar, vindo do formulário
        /// </summary>
        [BindProperty]
        public Utilizador Utilizador { get; set; } = new();

        /// <summary>
        /// Carrega o utilizador selecionado para edição
        /// </summary>
        public async Task<IActionResult> OnGetAsync(string id)
        {
            // vai buscar o utilizador pelo id
            var utilizador = await _userManager.FindByIdAsync(id);

            if (utilizador == null) return NotFound();

            Utilizador = utilizador;
            return Page();
        }

        /// <summary>
        /// Processa a atualização dos dados do utilizador através do UserManager
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Utilizador.PasswordHash");

            if (!ModelState.IsValid) return Page();

            // vai buscar o utilizador atual para atualizar
            var utilizador = await _userManager.FindByIdAsync(Utilizador.Id);
            if (utilizador == null) return NotFound();

            // atualiza os campos
            utilizador.Nome = Utilizador.Nome;
            utilizador.Email = Utilizador.Email;
            utilizador.UserName = Utilizador.Email;
            utilizador.Tipo = Utilizador.Tipo;
            utilizador.Telefone = Utilizador.Telefone;
            utilizador.DataNascimento = Utilizador.DataNascimento;

            await _userManager.UpdateAsync(utilizador);

            return RedirectToPage("Index");
        }
    }
}