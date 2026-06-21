using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Sensores
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
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
            // vai buscar o sensor pelo id para garantir que existe
            var sensor = await _context.Sensores.FindAsync(Sensor.Id);

            if (sensor != null)
            {
                // apaga primeiro os dados de monitorização associados a este sensor,
                // já que a relação Sensor -> DadosMonitorizacao não tem cascade automático
                var dadosAssociados = await _context.DadosMonitorizacao
                                                     .Where(d => d.FKSensor == sensor.Id)
                                                     .ToListAsync();

                if (dadosAssociados.Any())
                {
                    _context.DadosMonitorizacao.RemoveRange(dadosAssociados);
                }

                _context.Sensores.Remove(sensor);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }
}