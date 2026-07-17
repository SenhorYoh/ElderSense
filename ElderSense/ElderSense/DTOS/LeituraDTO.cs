namespace ElderSense.DTOs
{
    /// <summary>
    /// DTO de saída que representa uma leitura devolvida pela API,
    /// sem expor as navegações internas da entidade DadosMonitorizacao
    /// </summary>
    public class LeituraDto
    {
        /// <summary>
        /// Identificador da leitura
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Data e hora da leitura, guardada em UTC
        /// </summary>
        public DateTime DataHora { get; set; }

        /// <summary>
        /// Tipo de dado recolhido, ex: movimento, temperatura, bpm
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Valor associado ao tipo de dado
        /// </summary>
        public string Valor { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do sensor que gerou a leitura
        /// </summary>
        public int FKSensor { get; set; }

        /// <summary>
        /// Identificador do idoso a quem a leitura pertence
        /// </summary>
        public string FKUtilizador { get; set; } = string.Empty;
    }
}