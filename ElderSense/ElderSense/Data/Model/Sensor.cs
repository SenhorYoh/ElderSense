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

        //relacionamento do utilizador
        [ForeignKey(nameof(Utilizador))]
        public Utilizador FKUtilizador { get; set; }
    }
}
