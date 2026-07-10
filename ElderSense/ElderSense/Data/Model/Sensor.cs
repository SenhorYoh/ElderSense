using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ElderSense.Data.Model;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// lista dos tipos de hardware físico simulado,
    /// cada tipo tem capacidades diferentes de deteção
    /// </summary>
    public enum TipoSensor
    {
        /// <summary>
        /// Deteta presença/localização
        /// </summary>
        Beacon,

        /// <summary>
        /// Deteta sinais vitais (temperatura corporal, bpm)
        /// </summary>
        Pulseira,
    }

    /// <summary>
    /// classe dedicada aos sensores e suas informações
    /// </summary>
    public class Sensor
    {
        /// <summary>
        /// Identificador único do sensor
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Localização física do sensor (ex: Cozinha, Sala, Corpo do idoso)
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Localização")]
        public string Localizacao { get; set; } = "";

        /// <summary>
        /// Tipo de sensor (Beacon ou Pulseira)
        /// </summary>
        [Display(Name = "Tipo de Sensor")]
        [Required(ErrorMessage = "O {0} é obrigatório")]
        public TipoSensor Tipo { get; set; }

        /// <summary>
        /// Indica se o sensor está ativo
        /// </summary>
        [Display(Name = "Estado")]
        public bool Estado { get; set; }

        /// <summary>
        /// FK do Cuidador responsável pelo sensor
        /// Relacionamento 1-N com a classe Utilizador (Regra 4)
        /// </summary>
        [Display(Name = "Responsável")]
        public string FKUtilizador { get; set; } = "";

        /// <summary>
        /// Navigation property para o Utilizador (Cuidador) responsável
        /// </summary>
        [ForeignKey("FKUtilizador")]
        public Utilizador Utilizador { get; set; } = null!;

        /// <summary>
        /// FK do idoso a quem a pulseira pertence (aplicável apenas a sensores do tipo Pulseira)
        /// </summary>
        [Display(Name = "Idoso")]
        public string? FKIdoso { get; set; }

        /// <summary>
        /// Navigation property para o idoso associado ao sensor
        /// </summary>
        [ForeignKey("FKIdoso")]
        public Utilizador? IdosoAssociado { get; set; }
        /// <summary>
        /// Indica se o sensor foi arquivado (soft delete). 
        /// Sensores arquivados não aparecem nas listagens normais, 
        /// mas as suas leituras são preservadas como histórico consultável
        /// </summary>
        [Display(Name = "Arquivado")]
        public bool Arquivado { get; set; } = false;
    }
}