using ElderSense.Data;
using ElderSense.Data.Model;
using ElderSense.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Alertas
{
    /// <summary>
    /// Página de visualização dos Alertas. Os dois tipos de utilizadores podem consultar as informações
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        /// <summary>
        /// Contexto da base de dados
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Gestor de utilizadores do Identity, usado para identificar o utilizador autenticado
        /// </summary>
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public IndexModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lista de alertas a mostrar, ordenada do mais recente para o mais antigo
        /// </summary>
        public IList<Alerta> Alertas { get; set; } = [];

        /// <summary>
        /// Número total de alertas
        /// </summary>
        public int TotalAlertas => Alertas.Count;

        /// <summary>
        /// Número de alertas gerados hoje (comparação feita em hora de Portugal)
        /// </summary>
        public int AlertasHoje => Alertas.Count(a => a.DataHora.ParaHoraPortugal().Date == DateTime.UtcNow.ParaHoraPortugal().Date);

        /// <summary>
        /// Número de alertas gerados esta semana, a contar de segunda-feira (hora de Portugal)
        /// </summary>
        public int AlertasEstaSemana
        {
            get
            {
                var hojePortugal = DateTime.UtcNow.ParaHoraPortugal().Date;

                // recua até à segunda-feira desta semana
                int diasDesdeSegunda = ((int)hojePortugal.DayOfWeek + 6) % 7;
                var inicioSemana = hojePortugal.AddDays(-diasDesdeSegunda);

                return Alertas.Count(a => a.DataHora.ParaHoraPortugal().Date >= inicioSemana);
            }
        }

        /// <summary>
        /// True se o Cuidador autenticado ainda não tiver nenhum Idoso associado
        /// </summary>
        public bool BloqueiaAcesso { get; set; } = false;

        /// <summary>
        /// Carrega a lista de alertas. Se o utilizador for Cuidador sem idosos associados,
        /// bloqueia o acesso à listagem
        /// </summary>
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

            Alertas = await _context.Alertas
                .Include(a => a.IdosoAssociado)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }
    }
}