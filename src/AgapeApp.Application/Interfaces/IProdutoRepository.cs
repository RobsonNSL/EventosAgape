using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de produtos.
/// </summary>
public interface IProdutoRepository
{
    void Adicionar(Produto produto);

    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtém o produto para alteração (entidade rastreada pelo Entity Framework).</summary>
    Task<Produto?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Produto>> ListarAsync(bool incluirInativos = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
