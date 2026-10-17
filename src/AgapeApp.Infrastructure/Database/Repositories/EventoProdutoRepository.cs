using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class EventoProdutoRepository : IEventoProdutoRepository
{
    private readonly AgapeDbContext _context;

    public EventoProdutoRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(EventoProduto eventoProduto) => _context.EventoProdutos.Add(eventoProduto);

    public Task<EventoProduto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.EventoProdutos
            .AsNoTracking()
            .FirstOrDefaultAsync(eventoProduto => eventoProduto.Id == id, cancellationToken);

    // Sem AsNoTracking: a entidade é alterada em memória e persistida na unidade de trabalho.
    public Task<EventoProduto?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.EventoProdutos
            .FirstOrDefaultAsync(eventoProduto => eventoProduto.Id == id, cancellationToken);

    public Task<EventoProduto?> ObterPorEventoEProdutoAsync(
        Guid eventoId,
        Guid produtoId,
        CancellationToken cancellationToken = default)
        => _context.EventoProdutos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                eventoProduto => eventoProduto.EventoId == eventoId && eventoProduto.ProdutoId == produtoId,
                cancellationToken);

    public async Task<IReadOnlyList<EventoProduto>> ListarPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => await _context.EventoProdutos
            .AsNoTracking()
            .Where(eventoProduto => eventoProduto.EventoId == eventoId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EventoProduto>> ObterPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();

        if (lista.Count == 0)
            return [];

        return await _context.EventoProdutos
            .AsNoTracking()
            .Where(eventoProduto => lista.Contains(eventoProduto.Id))
            .ToListAsync(cancellationToken);
    }

    // Sem AsNoTracking: o estoque é baixado em memória e persistido na unidade de trabalho.
    public async Task<IReadOnlyList<EventoProduto>> ObterParaAtualizarPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();

        if (lista.Count == 0)
            return [];

        return await _context.EventoProdutos
            .Where(eventoProduto => lista.Contains(eventoProduto.Id))
            .ToListAsync(cancellationToken);
    }
}
