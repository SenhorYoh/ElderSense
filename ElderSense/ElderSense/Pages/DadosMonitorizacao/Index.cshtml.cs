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
        /// Lista de dados de monitorização, mais recentes primeiro
        /// </summary>
        public IList<Data.Model.DadosMonitorizacao> Dados { get; set; } = [];

        /// <summary>
        /// Número total de registos de monitorização
        /// </summary>
        public int TotalDados => Dados.Count;

        /// <summary>
        /// Número de registos recolhidos hoje
        /// </summary>
        public int DadosHoje => Dados.Count(d => d.DataHora.Date == DateTime.Today);

        /// <summary>
        /// Número de registos recolhidos esta semana
        /// </summary>
        public int DadosEstaSemana => Dados.Count(d => d.DataHora >= DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1));

        /// <summary>
        /// True se o Cuidador autenticado ainda não tiver nenhum Idoso associado
        /// </summary>
        public bool BloqueiaAcesso { get; set; } = false;

        /// <summary>
        /// Carrega a lista de dados de monitorização. Se o utilizador for Cuidador sem idosos
        /// associados, bloqueia o acesso à listagem
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

            Dados = await _context.DadosMonitorizacao
                .Include(d => d.Utilizador)
                .Include(d => d.Sensor)
                    .ThenInclude(s => s.IdosoAssociado)
                .OrderByDescending(d => d.DataHora)
                .ToListAsync();
        }
    }
}