using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ElderSense.Data;
using ElderSense.Data.Model;

namespace ElderSense.Pages
{

    /// <summary>
    /// Página de criação/associação de idoso a conta do Cuidador.
    /// O cuidador deve ter pelo menos 1 idoso associado para adicionar sensores,
    /// senão tiver, deve associar um idoso
    /// </summary>
    [Authorize]
    public class AdicionarIdosoModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly ApplicationDbContext _context;

        public AdicionarIdosoModel(UserManager<Utilizador> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        // classe auxiliar só para apanhar os dados deste formulário
        public class InputModel
        {
            [Required(ErrorMessage = "O nome do idoso é obrigatório.")]
            [Display(Name = "Nome do Idoso")]
            public string Nome { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Vai buscar o Cuidador logado e a sua lista
            var userId = _userManager.GetUserId(User);
            var cuidador = await _context.Set<Utilizador>()
                .Include(u => u.ListadeIdosos)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (cuidador == null) return NotFound();

            // Cria o identificador único para enganar o Identity
            var codigoUnico = Guid.NewGuid().ToString().Substring(0, 6);
            var emailFalso = $"idoso_{codigoUnico}@eldersense.local";
            var usernameUnico = Input.Nome.Replace(" ", "") + codigoUnico;

            //Prepara o novo perfil de Idoso
            var novoIdoso = new Utilizador
            {
                UserName = usernameUnico,
                Email = emailFalso,
                EmailConfirmed = true, // Para não pedir verificações de email
                Nome = Input.Nome 
            };

            // Cria a conta no Identity com uma password forte padrão que ninguém vai usar
            var result = await _userManager.CreateAsync(novoIdoso, "ElderSense_2026!");

            if (result.Succeeded)
            {
                // Liga o Idoso recém-criado ao Cuidador
                cuidador.ListadeIdosos.Add(novoIdoso);
                await _context.SaveChangesAsync();

                // Manda o cuidador de volta para os sensores, onde o bloqueio amarelo já terá desaparecido!
                return RedirectToPage("/Sensores/Create");
            }

            // Se der erro a criar (ex: o Identity reclama de algo), mostra no ecrã
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}