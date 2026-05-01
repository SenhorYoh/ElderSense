using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// classe dedicada aos sensores e suas informações
    /// </summary>
    public class Sensor{

        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        [Display(Name = "Localização")]
        public string Localizacao { get; set; } = "";

        [Display(Name = "Estado")]
        public bool Estado { get; set; }


        /// <summary>
        /// Relacionamento 1-N com a classe Utilizador
        /// </summary>
        [Display(Name = "Responsável")]
        [ForeignKey(nameof(Utilizador))]
        public int FKUtilizador { get; set; }
    }
}
