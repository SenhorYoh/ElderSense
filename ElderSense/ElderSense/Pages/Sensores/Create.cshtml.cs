using System;
using System.Threading.Tasks; // 🌟 GARANTE QUE ESTE USING ESTÁ AQUI
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ElderSense.Data;
using ElderSense.Data.Model;

namespace ElderSense.Pages.Sensores
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Sensor Sensor { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Vai buscar o utilizador atualmente logado no sistema
            var user = await _userManager.GetUserAsync(User);

            // Segurança extra: se a sessão expirou ou o user é nulo, manda para o Login
            if (user == null)
            {
                return Challenge();
            }

            // 2. Associa o sensor ao ID do utilizador (Garante que na classe 'Sensor' a propriedade se chama 'FKUtilizador')
            Sensor.FKUtilizador = user.Id;

            // 3. Remove as validações automáticas do ModelState que costumam bloquear o formulário
            ModelState.Remove("Sensor.Utilizador");
            ModelState.Remove("Sensor.FKUtilizador"); // Remove esta também, já que a preenchemos via código e não via HTML

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 4. Grava na Base de Dados
            _context.Sensores.Add(Sensor);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}