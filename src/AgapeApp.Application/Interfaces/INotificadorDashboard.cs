namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Notifica o dashboard do administrador em tempo real (SignalR) quando uma operação
/// é registrada em um evento (recarga ou venda).
/// </summary>
public interface INotificadorDashboard
{
    Task NotificarAsync(
        Guid eventoId,
        string tipo,
        object dados,
        CancellationToken cancellationToken = default);
}
