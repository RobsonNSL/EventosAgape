using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de recargas.
/// </summary>
public interface IRecargaRepository
{
    void Adicionar(Recarga recarga);

    /// <summary>
    /// Busca pela chave de idempotência da operação (retry ou toque duplo do app).
    /// </summary>
    Task<Recarga?> ObterPorIdOperacaoAsync(
        Guid idOperacaoCliente,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recarga>> ListarPorEventoAsync(
        Guid eventoId,
        int pular,
        int quantidade,
        CancellationToken cancellationToken = default);

    Task<int> ContarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    /// <summary>Lista todas as recargas do evento (sem paginação — uso nos relatórios).</summary>
    Task<IReadOnlyList<Recarga>> ListarTodasPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default);
}
