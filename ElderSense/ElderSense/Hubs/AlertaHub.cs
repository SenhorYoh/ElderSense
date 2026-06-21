using Microsoft.AspNetCore.SignalR;

namespace ElderSense.Hubs
{
    /// <summary>
    /// Hub do SignalR responsável por notificar os clientes ligados
    /// sempre que surge um novo alerta no sistema
    /// </summary>
    public class AlertaHub : Hub
    {
    }
}