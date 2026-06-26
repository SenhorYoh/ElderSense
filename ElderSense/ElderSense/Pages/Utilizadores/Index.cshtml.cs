using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Utilizadores
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // lista dos idosos associados ao cuidador autenticado
        public IList<Utilizador> Utilizadores { get; set; } = [];

        // estatística para o cabeçalho da página
        public int TotalIdosos => Utilizadores.Count;

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            // vai buscar o cuidador autenticado, já com a lista de idosos associados
            var cuidador = await _context.Utilizadores
                .Include(u => u.ListadeIdosos)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // se o cuidador existir, mostra só os idosos da sua rede
            Utilizadores = cuidador?.ListadeIdosos.ToList() ?? [];
        }
    }
}