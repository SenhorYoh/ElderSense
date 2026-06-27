using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.DadosMonitorizacao
{

    /// <summary>
    /// Página de visualização da monitorização dos dados. Os dois tipos de utilizadores podem visualizar
    /// </summary>
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

        // lista de dados de monitorização, mais recentes primeiro
        public IList<Data.Model.DadosMonitorizacao> Dados { get; set; } = [];

        // estatísticas para o cabeçalho da página
        public int TotalDados => Dados.Count;
        public int DadosHoje => Dados.Count(d => d.DataHora.Date == DateTime.Today);
        public int DadosEstaSemana => Dados.Count(d => d.DataHora >= DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1));

        // true se o Cuidador autenticado ainda não tiver nenhum Idoso associado
        public bool BloqueiaAcesso { get; set; } = false;

        public async Task OnGetAsync()
        {
            if (User.IsInRole("Cuidador"))
            {
                var userId = _userManager.GetUserId(User);
                var cuidador = await _context.Utilizadores
                    .Include(u => u.ListadeIdosos)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (cuidador == null || !cuidador.ListadeIdosos.Any())
                {
                    BloqueiaAcesso = true;
                    return;
                }
            }

            Dados = await _context.DadosMonitorizacao
                .Include(d => d.Utilizador)
                .Include(d => d.Sensor)
                    .ThenInclude(s => s.IdosoAssociado)
                .OrderByDescending(d => d.DataHora)
                .ToListAsync();
        }
    }
}