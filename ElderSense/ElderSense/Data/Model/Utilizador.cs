using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// lista dos tipos de utilizadores, idoso ou cuidador
    /// </summary>
    public enum TipoUtilizador
    {
        Idoso, //0
        Cuidador, //1
    }

    /// <summary>
    /// classe que representa os utilizadores da plataforma,
    /// engloba tanto idosos quanto cuidadores/familiares,
    /// distinguidos pelo tipo.
    /// O Utilizador herda o IdentityUser pois assim o utilizador do sistema (quem faz login)
    /// é igual ao utilizador do negócio (idoso/cuidador)
    /// </summary>
    public class Utilizador : IdentityUser{


        [Display(Name = "Utilizador")]
        [Required(ErrorMessage = "O {0} é obrigatório")]
        [StringLength(50)]
        public string Nome { get; set; } = "";


        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateOnly DataNascimento { get; set; }

        [Display(Name = "Tipo de Utilizador")]
        [Required(ErrorMessage = "O {0} é obrigatório")]
        public TipoUtilizador Tipo { get; set; }

        [StringLength(17)]
        [RegularExpression(@"\+?[0-9]{9,18}")] //telemóvel em Portugal, podemos adicionar formatos de paises depois
        public string? Telefone { get; set; }


        /// <summary>
        /// RELACIONAMENTO M:N cuidador <-> idoso
        /// </summary>

        //Se for idoso, esta lista guarda quem cuida
        public ICollection<Utilizador> ListadeCuidadores { get; set; } = [];

        //se for cuidador, esta lista guarda os idosos
        public ICollection<Utilizador> ListadeIdosos { get; set; } = [];
    }
}
