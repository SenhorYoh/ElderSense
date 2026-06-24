using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Sensores
{
    /// <summary>
    /// Página de visualização dos sensores. Os dois tipos de utilizadores podem visualizar
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // lista de sensores a mostrar na página
        public IList<Sensor> Sensores { get; set; } = [];

        public async Task OnGetAsync()
        {
            // busca todos os sensores incluindo o utilizador associado
            Sensores = await _context.Sensores
                .Include(s => s.Utilizador)
                .ToListAsync();
        }
    }
}
