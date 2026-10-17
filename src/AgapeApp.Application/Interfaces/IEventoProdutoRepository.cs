using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de produtos vinculados aos eventos (preço e estoque por evento).
/// </summary>
public interface IEventoProdutoRepository
{
    void Adicionar(EventoProduto eventoProduto);

    Task<EventoProduto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtém o vínculo para alteração (entidade rastreada pelo Entity Framework).</summary>
    Task<EventoProduto?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventoProduto?> ObterPorEventoEProdutoAsync(
        Guid eventoId,
        Guid produtoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventoProduto>> ListarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventoProduto>> ObterPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém vários vínculos rastreados pelo Entity Framework (baixa de estoque na venda).
    /// </summary>
    Task<IReadOnlyList<EventoProduto>> ObterParaAtualizarPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);
}
