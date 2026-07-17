namespace ElderSense.DTOs
{
    /// <summary>
    /// DTO de entrada com os dados enviados pelo hardware ao registar uma nova leitura
    /// </summary>
    public class CriarLeituraDto
    {
        /// <summary>
        /// Identificador do idoso a quem a leitura pertence
        /// </summary>
        public string IdosoId { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do sensor que gerou a leitura
        /// </summary>
        public int SensorId { get; set; }

        /// <summary>
        /// Tipo de dado recolhido, ex: movimento, temperatura, bpm
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Valor associado ao tipo de dado
        /// </summary>
        public string Valor { get; set; } = string.Empty;
    }
}
