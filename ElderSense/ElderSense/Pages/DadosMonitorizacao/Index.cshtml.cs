using ElderSense.Data;
using ElderSense.Data.Model;
using ElderSense.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        /// Número de registos recolhidos hoje (comparação feita em hora de Portugal)
        /// </summary>
        public int DadosHoje => Dados.Count(d => d.DataHora.ParaHoraPortugal().Date == DateTime.UtcNow.ParaHoraPortugal().Date);

        /// <summary>
        /// Número de registos recolhidos esta semana, a contar de segunda-feira (hora de Portugal)
        /// </summary>
        public int DadosEstaSemana
        {
            get
            {
                var hojePortugal = DateTime.UtcNow.ParaHoraPortugal().Date;

                // recua até à segunda-feira desta semana
                int diasDesdeSegunda = ((int)hojePortugal.DayOfWeek + 6) % 7;
                var inicioSemana = hojePortugal.AddDays(-diasDesdeSegunda);

                return Dados.Count(d => d.DataHora.ParaHoraPortugal().Date >= inicioSemana);
            }
        }

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

        /// <summary>
        /// Atualiza o tipo e/ou o valor de uma leitura existente.
        /// Só o Cuidador pode editar leituras; as FKs nunca são alteráveis
        /// </summary>
        public async Task<IActionResult> OnPostEditarAsync(int id, string? tipo, string? valor)
        {
            if (string.IsNullOrEmpty(tipo) && string.IsNullOrEmpty(valor))
                return RedirectToPage("Index");

            var leitura = await _context.DadosMonitorizacao.FindAsync(id);
            if (leitura == null) return NotFound();

            if (!string.IsNullOrEmpty(tipo)) leitura.Tipo = tipo;
            if (!string.IsNullOrEmpty(valor)) leitura.Valor = valor;

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        /// <summary>
        /// Elimina uma leitura, cortando primeiro as ligações M:N com os alertas.
        /// Só o Cuidador pode eliminar leituras
        /// </summary>
        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var leitura = await _context.DadosMonitorizacao
                .Include(d => d.ListadeAlertas)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (leitura == null) return NotFound();

            // corta as ligações na tabela intermédia antes de apagar (o lado dos dados está em Restrict)
            leitura.ListadeAlertas.Clear();

            _context.DadosMonitorizacao.Remove(leitura);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}