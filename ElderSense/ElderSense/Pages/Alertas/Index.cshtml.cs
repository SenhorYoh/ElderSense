using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Alertas
{
    /// <summary>
    /// Página de visualização dos Alertas. Os dois tipos de utilizadores podem consultas as informações
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // lista de alertas a mostrar, ordenada do mais recente para o mais antigo
        public IList<Alerta> Alertas { get; set; } = [];

        public async Task OnGetAsync()
        {
            Alertas = await _context.Alertas
                .Include(a => a.Utilizador)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }
    }
}