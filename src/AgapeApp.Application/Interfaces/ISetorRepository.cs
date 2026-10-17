using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de setores dos eventos.
/// </summary>
public interface ISetorRepository
{
    void Adicionar(Setor setor);

    Task<Setor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Obtém o setor para alteração (entidade rastreada pelo Entity Framework).</summary>
    Task<Setor?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExisteNoEventoAsync(
        Guid eventoId,
        string nome,
        Guid? ignorarSetorId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Setor>> ListarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Setor>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
