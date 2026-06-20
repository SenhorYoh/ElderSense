using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Utilizadores
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // lista de utilizadores a mostrar na página
        public IList<Utilizador> Utilizadores { get; set; } = [];

        public async Task OnGetAsync()
        {
            // busca todos os utilizadores
            Utilizadores = await _context.Utilizadores.ToListAsync();
        }
    }
}