using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Utilizadores
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly ApplicationDbContext _context;

        public DeleteModel(UserManager<Utilizador> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public Utilizador Utilizador { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // vai buscar o utilizador pelo id
            var utilizador = await _userManager.FindByIdAsync(id);

            if (utilizador == null) return NotFound();

            Utilizador = utilizador;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // vai buscar o utilizador pelo id, já com as relações M:N de Cuidador<->Idoso carregadas
            var utilizador = await _context.Utilizadores
                .Include(u => u.ListadeCuidadores)
                .Include(u => u.ListadeIdosos)
                .FirstOrDefaultAsync(u => u.Id == Utilizador.Id);

            if (utilizador != null)
            {
                // apaga os alertas diretamente associados a este idoso (FKIdoso é NoAction por defeito,
                // não pode ser cascade). Tem de vir antes dos Dados de Monitorização, já que um
                // Alerta pode estar ligado a Dados deste idoso através da relação M:N.
                var alertasDoIdoso = await _context.Alertas
                    .Where(a => a.FKIdoso == utilizador.Id)
                    .Include(a => a.ListadeDados)
                    .ToListAsync();

                foreach (var alerta in alertasDoIdoso)
                {
                    alerta.ListadeDados.Clear();
                }
                await _context.SaveChangesAsync();

                _context.Alertas.RemoveRange(alertasDoIdoso);
                await _context.SaveChangesAsync();

                // vai buscar todos os dados de monitorização deste utilizador,
                // já com a lista de alertas associados (relação M:N) carregada
                var dados = await _context.DadosMonitorizacao
                    .Where(d => d.FKUtilizador == utilizador.Id)
                    .Include(d => d.ListadeAlertas)
                    .ToListAsync();

                // remove manualmente a ligação na tabela de junção AlertaDadosMonitorizacao,
                // já que essa FK é Restrict (não pode ser cascade - ver ApplicationDbContext)
                foreach (var dado in dados)
                {
                    dado.ListadeAlertas.Clear();
                }
                await _context.SaveChangesAsync();

                // agora os DadosMonitorizacao já podem ser apagados sem violar a constraint
                _context.DadosMonitorizacao.RemoveRange(dados);
                await _context.SaveChangesAsync();

                // apaga os sensores associados a este idoso (FKIdoso é Restrict, não pode ser cascade -
                // ver comentário no ApplicationDbContext sobre multiple cascade paths). Tem de vir
                // depois de apagar os DadosMonitorizacao, já que estes apontavam para o Sensor (FKSensor é NoAction).
                var sensoresDoIdoso = await _context.Sensores
                    .Where(s => s.FKIdoso == utilizador.Id)
                    .ToListAsync();
                _context.Sensores.RemoveRange(sensoresDoIdoso);
                await _context.SaveChangesAsync();

                // limpa a relação M:N auto-referencial Cuidador<->Idoso (tabela UtilizadorUtilizador),
                // já que esta não tem cascade configurado e bloqueia o delete do utilizador
                utilizador.ListadeCuidadores.Clear();
                utilizador.ListadeIdosos.Clear();
                await _context.SaveChangesAsync();

                // apaga o utilizador - dispara os cascades automáticos para Sensores (do Cuidador) e Alertas
                await _userManager.DeleteAsync(utilizador);
            }

            return RedirectToPage("Index");
        }
    }
}