using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.DadosMonitorizacao
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // lista de dados de monitorização, mais recentes primeiro
        public IList<Data.Model.DadosMonitorizacao> Dados { get; set; } = [];

        public async Task OnGetAsync()
        {
            Dados = await _context.DadosMonitorizacao
                .Include(d => d.Utilizador)
                .Include(d => d.Sensor)
                    .ThenInclude(s => s.IdosoAssociado)
                .OrderByDescending(d => d.DataHora)
                .ToListAsync();
        }
    }
}