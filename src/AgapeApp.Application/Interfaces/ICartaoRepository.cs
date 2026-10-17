using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de cartões.
/// </summary>
public interface ICartaoRepository
{
    void Adicionar(Cartao cartao);

    Task<Cartao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Cartao?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>Obtém o cartão para alteração (entidade rastreada pelo Entity Framework).</summary>
    Task<Cartao?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistePorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cartao>> ListarAsync(string? codigo, bool incluirInativos = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cartao>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
