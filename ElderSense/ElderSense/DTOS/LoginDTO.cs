namespace ElderSense.DTOs
{
    /// <summary>
    /// DTO de entrada com as credenciais de login recebidas pela API
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// Email do utilizador (usado como nome de utilizador no login)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password do utilizador
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
