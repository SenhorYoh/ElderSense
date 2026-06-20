namespace ElderSense.API
{
    /// <summary>
    /// modelo para receber as credenciais de login via API
    /// </summary>
    public class LoginModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}