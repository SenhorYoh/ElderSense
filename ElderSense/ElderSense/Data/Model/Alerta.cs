using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElderSense.Data.Model
{

    /// <summary>
    /// classe Alerta, serve para mostrar os detalhes dum alerta
    /// devido a uma quebra de rotina, dados de monitorização repetidos...
    /// </summary>
    public class Alerta {

        [Key]
        public int Id { get; set; }

        //data e hora do alerta
        public DateTime DataHora { get; set; } = DateTime.Now;

        //descrição do alerta Ex: quebra de rotina
        [StringLength(100)]
        public string Mensagem { get; set; } = "";

        //estado?

        /// <summary>
        /// RELACIONAMENTO 1-N obrigatório com o Utilizador
        /// </summary>

        [Display(Name = "Responsável")]
        [ForeignKey(nameof(Utilizador))]
        public int FKUtilizador { get; set; }

        /// <summary>
        /// RELACIONAMENTO N:M COM DadosMonitorizacao
        /// </summary>
        public ICollection<DadosMonitorizacao> ListadeDados { get ; set; } = [];
    }
}
