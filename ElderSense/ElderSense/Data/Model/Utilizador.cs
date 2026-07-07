using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ElderSense.Data.Model
{
    /// <summary>
    /// lista dos tipos de utilizadores, idoso ou cuidador
    /// </summary>
    public enum TipoUtilizador
    {
        /// <summary>
        /// Utilizador responsável por cuidar de um ou mais idosos
        /// </summary>
        Cuidador,

        /// <summary>
        /// Utilizador idoso monitorizado pelo sistema
        /// </summary>
        Idoso,
    }

    /// <summary>
    /// classe que representa os utilizadores da plataforma,
    /// engloba tanto idosos quanto cuidadores/familiares,
    /// distinguidos pelo tipo.
    /// O Utilizador herda o IdentityUser pois assim o utilizador do sistema (quem faz login)
    /// é igual ao utilizador do negócio (idoso/cuidador)
    /// </summary>
    public class Utilizador : IdentityUser
    {
        /// <summary>
        /// Nome completo do utilizador
        /// </summary>
        [Display(Name = "Utilizador")]
        [Required(ErrorMessage = "O {0} é obrigatório")]
        [StringLength(50)]
        public string Nome { get; set; } = "";

        /// <summary>
        /// Data de nascimento do utilizador
        /// </summary>
        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateOnly DataNascimento { get; set; }

        /// <summary>
        /// Tipo de utilizador (Cuidador ou Idoso)
        /// </summary>
        [Display(Name = "Tipo de Utilizador")]
        [Required(ErrorMessage = "O {0} é obrigatório")]
        public TipoUtilizador Tipo { get; set; }

        /// <summary>
        /// Número de telemóvel do utilizador
        /// </summary>
        [StringLength(17)]
        [RegularExpression(@"\+?[0-9]{9,18}")] //telemóvel em Portugal, podemos adicionar formatos de paises depois
        public string? Telefone { get; set; }

        /// <summary>
        /// Relacionamento M:N cuidador &lt;-&gt; idoso (Regra 6).
        /// Se for idoso, esta lista guarda quem cuida dele
        /// </summary>
        public ICollection<Utilizador> ListadeCuidadores { get; set; } = [];

        /// <summary>
        /// Relacionamento M:N cuidador &lt;-&gt; idoso (Regra 6).
        /// Se for cuidador, esta lista guarda os idosos a seu cargo
        /// </summary>
        public ICollection<Utilizador> ListadeIdosos { get; set; } = [];
    }
}