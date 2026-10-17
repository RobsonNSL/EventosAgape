using AgapeApp.Api.Hubs;
using AgapeApp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AgapeApp.Api.Services;

/// <summary>
/// Publica as operações no grupo do evento no hub do dashboard (tempo real).
/// O cliente escuta o método "operacao" e recebe { tipo, eventoId, dados, em }.
/// </summary>
public class SignalRDashboardNotificador : INotificadorDashboard
{
    private readonly IHubContext<DashboardHub> _hubContext;

    public SignalRDashboardNotificador(IHubContext<DashboardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotificarAsync(
        Guid eventoId,
        string tipo,
        object dados,
        CancellationToken cancellationToken = default)
        => _hubContext.Clients
            .Group(DashboardHub.GrupoDoEvento(eventoId))
            .SendAsync(
                "operacao",
                new { tipo, eventoId, dados, em = DateTimeOffset.UtcNow },
                cancellationToken);
}
