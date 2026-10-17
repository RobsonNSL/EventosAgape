using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de vendas e seus itens.
/// </summary>
public interface IVendaRepository
{
    void Adicionar(Venda venda);

    void AdicionarItem(ItemVenda item);

    Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca pela chave de idempotência da operação (retry ou toque duplo do app).
    /// </summary>
    Task<Venda?> ObterPorIdOperacaoAsync(Guid idOperacaoCliente, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemVenda>> ListarItensPorVendaAsync(Guid vendaId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemVenda>> ListarItensPorVendasAsync(
        IEnumerable<Guid> vendaIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Venda>> ListarPorEventoAsync(
        Guid eventoId,
        int pular,
        int quantidade,
        CancellationToken cancellationToken = default);

    Task<int> ContarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    // Relatórios (sem paginação — uso interno do resumo e da exportação para Excel).
    Task<IReadOnlyList<Venda>> ListarTodasPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemVenda>> ListarTodosItensPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default);
}
