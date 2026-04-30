using System.ComponentModel.DataAnnotations;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// lista dos tipos de utilizadores, idoso ou cuidador
    /// </summary>
    public enum TipoUtilizador
    {
        Idoso,
        Cuidador
    }

    /// <summary>
    /// classe que representa os utilizadores da plataforma,
    /// engloba tanto idosos quanto cuidadores/familiares,
    /// distinguidos pelo tipo
    /// </summary>
    public class Utilizador{

        [Key]
        public int Id { get; set; }

        [Display(Name = "Utilizador")]
        [Required(ErrorMessage = "O {0} é obrigatório")]
        [StringLength(50)]
        public string Nome { get; set; } = "";


        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateOnly DataNascimento { get; set; }

        [StringLength(50)]
        public TipoUtilizador Tipo { get; set; }

        [Required(ErrorMessage = "O número de {0} é obrigatório")]
        [StringLength(17)]
        [RegularExpression(@"\+?[0-9]{9,18}")] //telemóvel em Portugal, podemos adicionar formatos de paises depois
        public string Telefone { get; set; } = "";
    }
}
