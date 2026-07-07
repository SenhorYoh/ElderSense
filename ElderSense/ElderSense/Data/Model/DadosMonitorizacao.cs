using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// classe dos dados da monitorização do idoso
    /// </summary>
    public class DadosMonitorizacao
    {
        /// <summary>
        /// Identificador único do registo de monitorização
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data e hora do registo
        /// </summary>
        public DateTime DataHora { get; set; } = DateTime.Now;

        /// <summary>
        /// Tipo de dado recolhido, ex: movimento, porta, temperatura, bpm
        /// </summary>
        [StringLength(50)]
        public string Tipo { get; set; } = "";

        /// <summary>
        /// Valor associado ao tipo de dado, ex: aberta, ausente 2h
        /// </summary>
        [StringLength(100)]
        public string Valor { get; set; } = "";

        /// <summary>
        /// Relacionamento 1-N obrigatório com a classe Utilizador (Regra 4)
        /// </summary>
        [Display(Name = "Responsável")]
        [ForeignKey(nameof(Utilizador))]
        public string FKUtilizador { get; set; } = "";

        /// <summary>
        /// Navigation property para o Utilizador a quem pertencem os dados
        /// </summary>
        public Utilizador Utilizador { get; set; } = null!;

        /// <summary>
        /// Relacionamento 1-N obrigatório com a classe Sensor (Regra 4)
        /// </summary>
        [Display(Name = "Sensor")]
        public int FKSensor { get; set; }

        /// <summary>
        /// Navigation property para o Sensor que gerou o registo
        /// </summary>
        public Sensor Sensor { get; set; } = null!;

        /// <summary>
        /// Relacionamento M:N com Alerta (Regra 6)
        /// </summary>
        public ICollection<Alerta> ListadeAlertas { get; set; } = [];
    }
}