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
                // vai buscar os dados de monitorização associados a este sensor
                var dadosAssociados = await _context.DadosMonitorizacao
                                                     .Where(d => d.FKSensor == sensor.Id)
                                                     .ToListAsync();

                if (dadosAssociados.Any())
                {
                    var idsDados = dadosAssociados.Select(d => d.Id).ToList();

                    // vai buscar todos os Alertas ligados a estes dados específicos
                    var alertasLigados = await _context.Alertas
                                                        .Include(a => a.ListadeDados)
                                                        .Where(a => a.ListadeDados.Any(d => idsDados.Contains(d.Id)))
                                                        .ToListAsync();

                    foreach (var alerta in alertasLigados)
                    {
                        // remove a ligação aos dados deste sensor
                        var dadosParaRemover = alerta.ListadeDados.Where(d => idsDados.Contains(d.Id)).ToList();
                        foreach (var dado in dadosParaRemover)
                        {
                            alerta.ListadeDados.Remove(dado);
                        }

                        // se o alerta ficou sem qualquer dado associado, deixa de ter sentido mantê-lo
                        if (!alerta.ListadeDados.Any())
                        {
                            _context.Alertas.Remove(alerta);
                        }
                    }

                    // agora já pode apagar os dados de monitorização sem violar a tabela junction
                    _context.DadosMonitorizacao.RemoveRange(dadosAssociados);
                }

                _context.Sensores.Remove(sensor);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }
}