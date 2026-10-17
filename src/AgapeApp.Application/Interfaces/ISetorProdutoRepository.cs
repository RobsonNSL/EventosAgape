using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos vínculos setor × produto do evento (o que cada setor vende).
/// </summary>
public interface ISetorProdutoRepository
{
    void Adicionar(SetorProduto setorProduto);

    void Remover(SetorProduto setorProduto);

    Task<SetorProduto?> ObterAsync(
        Guid setorId,
        Guid eventoProdutoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SetorProduto>> ListarPorSetorAsync(Guid setorId, CancellationToken cancellationToken = default);
}
