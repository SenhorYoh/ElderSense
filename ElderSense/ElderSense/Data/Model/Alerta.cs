using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// classe Alerta, serve para mostrar os detalhes dum alerta
    /// devido a uma quebra de rotina, dados de monitorização repetidos...
    /// </summary>
    public class Alerta
    {
        [Key]
        public int Id { get; set; }

        // data e hora do alerta
        public DateTime DataHora { get; set; } = DateTime.Now;

        // descrição do alerta Ex: quebra de rotina
        [StringLength(100)]
        public string Mensagem { get; set; } = "";

        /// <summary>
        /// Relacionamento 1-N obrigatório com o Utilizador (Regra 4)
        /// </summary>
        [Display(Name = "Responsável")]
        [ForeignKey(nameof(Utilizador))]
        public string FKUtilizador { get; set; } = "";

        // navigation property para o Utilizador
        public Utilizador Utilizador { get; set; } = null!;

        /// <summary>
        /// Relacionamento M:N com DadosMonitorizacao (Regra 6)
        /// </summary>
        public ICollection<DadosMonitorizacao> ListadeDados { get; set; } = [];
    }
}