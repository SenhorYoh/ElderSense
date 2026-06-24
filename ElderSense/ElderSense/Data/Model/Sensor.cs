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
        Beacon,   //0 - deteta presença/localização
        Pulseira, //1 - deteta sinais vitais (temperatura corporal, bpm)
    }

    /// <summary>
    /// classe dedicada aos sensores e suas informações
    /// </summary>
    public class Sensor
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        [Display(Name = "Localização")]
        public string Localizacao { get; set; } = "";

        [Display(Name = "Tipo de Sensor")]
        [Required(ErrorMessage = "O {0} é obrigatório")]
        public TipoSensor Tipo { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; }

        // ==========================================
        // 1. O DONO DO SENSOR (O CUIDADOR)
        // ==========================================

        /// <summary>
        /// Relacionamento 1-N com a classe Utilizador (Regra 4)
        /// </summary>
        [Display(Name = "Responsável")]
        public string FKUtilizador { get; set; } = "";

        // navigation property para o Utilizador
        [ForeignKey("FKUtilizador")]
        public Utilizador Utilizador { get; set; } = null!;

        // ==========================================
        // 2. A QUEM A PULSEIRA PERTENCE (IDOSO)
        // ==========================================
        [Display(Name = "Idoso")]
        public string? FKIdoso { get; set; }

        [ForeignKey("FKIdoso")]
        public Utilizador? IdosoAssociado { get; set; }
    }
}