using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Sensores
{

    /// <summary>
    /// Página de edição dos sensores. Apenas um utilizador logado e do tipo Cuidador pode edição
    /// </summary>
    /// 

    [Authorize(Roles = "Cuidador")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Sensor Sensor { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // vai buscar o sensor pelo id
            var sensor = await _context.Sensores.FindAsync(id);

            // se não existir, redireciona para a lista
            if (sensor == null) return NotFound();

            Sensor = sensor;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // remove a validação da navigation property pois não vem do formulário
            ModelState.Remove("Sensor.Utilizador");

            if (!ModelState.IsValid) return Page();

            // atualiza o sensor na BD
            _context.Sensores.Update(Sensor);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}