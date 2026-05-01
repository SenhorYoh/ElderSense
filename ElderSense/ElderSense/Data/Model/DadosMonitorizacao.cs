using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// classe dos dados da monitorização do idoso
    /// </summary>
    public class DadosMonitorizacao{

        [Key]
        public int Id { get; set; }

        //data e hora do registo
        public DateTime DataHora { get; set; } = DateTime.Now;

        //ex: movimento, porta
        [StringLength(50)]
        public string Tipo { get; set; } = "";

        //ex: aberta, ausente 2h
        [StringLength(100)]
        public string Valor { get; set; } = "";

        /// <summary>
        /// Relacionamento 1-N obrigatório com a classe Utilizador
        /// </summary>
        [Display(Name = "Responsável")]
        [ForeignKey(nameof(Utilizador))]
        public int FKUtilizador { get; set; }

        /// <summary>
        /// Relacionamento 1-N obrigatório com a classe Sensor
        /// </summary>

        [Display(Name = "Sensor")]
        [ForeignKey(nameof(Sensor))]
        public int FKSensor { get; set; }

    }
}
