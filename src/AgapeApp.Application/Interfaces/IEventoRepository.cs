using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de eventos.
/// </summary>
public interface IEventoRepository
{
    void Adicionar(Evento evento);

    Task<Evento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtém o evento para alteração (entidade rastreada pelo Entity Framework).</summary>
    Task<Evento?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Evento>> ListarAsync(SituacaoEvento? situacao = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Evento>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
