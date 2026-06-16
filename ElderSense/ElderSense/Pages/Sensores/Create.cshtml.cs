using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
            // associa o sensor ao utilizador atual
            var user = await _userManager.GetUserAsync(User);
            Sensor.FKUtilizador = user!.Id;

            // remove a validação da navigation property pois não vem do formulário
            ModelState.Remove("Sensor.Utilizador");

            if (!ModelState.IsValid) return Page();

            _context.Sensores.Add(Sensor);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}