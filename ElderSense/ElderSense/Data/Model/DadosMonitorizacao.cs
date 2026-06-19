using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// classe dos dados da monitorização do idoso
    /// </summary>
    public class DadosMonitorizacao
    {
        [Key]
        public int Id { get; set; }

        // data e hora do registo
        public DateTime DataHora { get; set; } = DateTime.Now;

        // ex: movimento, porta
        [StringLength(50)]
        public string Tipo { get; set; } = "";

        // ex: aberta, ausente 2h
        [StringLength(100)]
        public string Valor { get; set; } = "";

        /// <summary>
        /// Relacionamento 1-N obrigatório com a classe Utilizador (Regra 4)
        /// </summary>
        [Display(Name = "Responsável")]
        public string FKUtilizador { get; set; } = "";

        // navigation property para o Utilizador
        [ForeignKey("FKUtilizador")]
        public Utilizador Utilizador { get; set; } = null!;

        /// <summary>
        /// Relacionamento 1-N obrigatório com a classe Sensor (Regra 4)
        /// </summary>
        [Display(Name = "Sensor")]
        public int FKSensor { get; set; }

        // navigation property para o Sensor
        [ForeignKey("FKSensor")]
        public Sensor Sensor { get; set; } = null!;

        /// <summary>
        /// Relacionamento M:N com Alerta (Regra 6)
        /// </summary>
        public ICollection<Alerta> ListadeAlertas { get; set; } = [];
    }
}