using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso às designações de operadores nos eventos.
/// </summary>
public interface IEventoOperadorRepository
{
    void Adicionar(EventoOperador designacao);

    Task<EventoOperador?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtém a designação para alteração (entidade rastreada pelo Entity Framework).</summary>
    Task<EventoOperador?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventoOperador?> ObterPorEventoEUsuarioAsync(
        Guid eventoId,
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventoOperador>> ListarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventoOperador>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
