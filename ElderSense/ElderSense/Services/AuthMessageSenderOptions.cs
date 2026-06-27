namespace ElderSense.Services
{
    /// <summary>
    /// Opções de configuração para o envio de emails via SendGrid,
    /// lidas a partir dos User Secrets / appsettings
    /// </summary>
    public class AuthMessageSenderOptions
    {
        /// <summary>
        /// Chave de API do SendGrid
        /// </summary>
        public string? SendGridKey { get; set; }

        /// <summary>
        /// Email de origem usado para enviar as mensagens
        /// </summary>
        public string? SendGridEmail { get; set; }
    }
}