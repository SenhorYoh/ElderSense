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
        public string tipo { get; set; } = "";

        //ex: aberta, ausente
        public string valor { get; set; } = "";

        //relacionamento utilizador
        [ForeignKey(nameof(Utilizador))]
        public Utilizador FKUtilizador { get; set; }


    }
}
