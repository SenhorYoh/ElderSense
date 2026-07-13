namespace ElderSense.DTOs
{
    /// <summary>
    /// DTO de entrada com os campos alteráveis de uma leitura existente
    /// </summary>
    public class AtualizarLeituraDto
    {
        /// <summary>
        /// Novo tipo de dado (opcional)
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Novo valor associado (opcional)
        /// </summary>
        public string Valor { get; set; } = string.Empty;
    }
}
