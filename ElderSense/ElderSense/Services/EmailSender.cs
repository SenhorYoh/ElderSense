using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ElderSense.Services;

/// <summary>
/// Serviço responsável por enviar emails através da API do SendGrid,
/// usado pelo Identity para confirmação de conta e outras notificações
/// </summary>
public class EmailSender : IEmailSender
{
    /// <summary>
    /// Logger usado para registar o sucesso ou falha do envio de emails
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Construtor que recebe as opções de configuração do SendGrid e o logger, injetados pelo sistema
    /// </summary>
    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
    }

    /// <summary>
    /// Opções de configuração do SendGrid (chave da API e email de origem)
    /// </summary>
    public AuthMessageSenderOptions Options { get; }

    /// <summary>
    /// Envia um email usando a chave do SendGrid configurada
    /// </summary>
    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            throw new Exception("A chave do SendGrid (SendGridKey) não está configurada.");
        }
        await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    /// <summary>
    /// Monta e envia a mensagem através do cliente do SendGrid, e regista o resultado no log
    /// </summary>
    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var fromEmail = Options.SendGridEmail;
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(fromEmail, "ElderSense"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Desativa o rastreio de cliques para maior privacidade (opcional)
        msg.SetClickTracking(false, false);

        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode
                               ? $"Email para {toEmail} enviado com sucesso!"
                               : $"Falha ao enviar email para {toEmail}");
    }
}