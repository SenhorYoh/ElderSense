using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// classe Alerta, serve para mostrar os detalhes dum alerta
    /// devido a uma quebra de rotina ou a dados de monitorização fora do normal
    /// </summary>
    public class Alerta
    {
        /// <summary>
        /// Identificador único do alerta
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data e hora em que o alerta foi gerado
        /// </summary>
        public DateTime DataHora { get; set; } = DateTime.Now;

        /// <summary>
        /// Descrição do alerta, Ex: quebra de rotina
        /// </summary>
        [StringLength(100)]
        public string Mensagem { get; set; } = "";

        /// <summary>
        /// Relacionamento 1-N obrigatório com o Utilizador (Regra 4)
        /// </summary>
        [Display(Name = "Responsável")]
        [ForeignKey(nameof(Utilizador))]
        public string FKUtilizador { get; set; } = "";

        /// <summary>
        /// Navigation property para o Utilizador responsável (Cuidador) pelo alerta
        /// </summary>
        public Utilizador Utilizador { get; set; } = null!;

        /// <summary>
        /// FK do idoso a quem pertence o alerta
        /// </summary>
        [Display(Name = "Idoso")]
        public string? FKIdoso { get; set; }

        /// <summary>
        /// Navigation property para o idoso associado ao alerta
        /// </summary>
        [ForeignKey("FKIdoso")]
        public Utilizador? IdosoAssociado { get; set; }

        /// <summary>
        /// Relacionamento M:N com DadosMonitorizacao (Regra 6)
        /// </summary>
        public ICollection<DadosMonitorizacao> ListadeDados { get; set; } = [];
    }
}