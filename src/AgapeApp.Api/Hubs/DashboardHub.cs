using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AgapeApp.Api.Hubs;

/// <summary>
/// Hub do dashboard do administrador: entrega as operações (recargas/vendas) em tempo real.
/// O acesso exige perfil de administrador; no WebSocket o token é enviado
/// na query string (?access_token=...), lido pelo JwtBearerEvents.OnMessageReceived.
/// </summary>
[Authorize(Roles = "Administrador")]
public class DashboardHub : Hub
{
    /// <summary>Nome do grupo de um evento (uma conexão pode acompanhar vários eventos).</summary>
    public static string GrupoDoEvento(Guid eventoId) => $"evento-{eventoId}";

    /// <summary>Passa a receber as operações de um evento.</summary>
    public Task EntrarNoEvento(Guid eventoId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GrupoDoEvento(eventoId));

    /// <summary>Deixa de receber as operações de um evento.</summary>
    public Task SairDoEvento(Guid eventoId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoDoEvento(eventoId));
}
