using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class VendaRepository : IVendaRepository
{
    private readonly AgapeDbContext _context;

    public VendaRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(Venda venda) => _context.Vendas.Add(venda);

    public void AdicionarItem(ItemVenda item) => _context.ItensVenda.Add(item);

    public Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Vendas
            .AsNoTracking()
            .FirstOrDefaultAsync(venda => venda.Id == id, cancellationToken);

    public Task<Venda?> ObterPorIdOperacaoAsync(
        Guid idOperacaoCliente,
        CancellationToken cancellationToken = default)
        => _context.Vendas
            .AsNoTracking()
            .FirstOrDefaultAsync(venda => venda.IdOperacaoCliente == idOperacaoCliente, cancellationToken);

    public async Task<IReadOnlyList<ItemVenda>> ListarItensPorVendaAsync(
        Guid vendaId,
        CancellationToken cancellationToken = default)
        => await _context.ItensVenda
            .AsNoTracking()
            .Where(item => item.VendaId == vendaId)
            .OrderBy(item => item.CriadoEm)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ItemVenda>> ListarItensPorVendasAsync(
        IEnumerable<Guid> vendaIds,
        CancellationToken cancellationToken = default)
    {
        var lista = vendaIds.Distinct().ToList();

        if (lista.Count == 0)
            return [];

        return await _context.ItensVenda
            .AsNoTracking()
            .Where(item => lista.Contains(item.VendaId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Venda>> ListarPorEventoAsync(
        Guid eventoId,
        int pular,
        int quantidade,
        CancellationToken cancellationToken = default)
        => await _context.Vendas
            .AsNoTracking()
            .Where(venda => venda.EventoId == eventoId)
            .OrderByDescending(venda => venda.CriadoEm)
            .Skip(pular)
            .Take(quantidade)
            .ToListAsync(cancellationToken);

    public Task<int> ContarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
        => _context.Vendas
            .AsNoTracking()
            .CountAsync(venda => venda.EventoId == eventoId, cancellationToken);

    public async Task<IReadOnlyList<Venda>> ListarTodasPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => await _context.Vendas
            .AsNoTracking()
            .Where(venda => venda.EventoId == eventoId)
            .OrderBy(venda => venda.CriadoEm)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ItemVenda>> ListarTodosItensPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => await _context.ItensVenda
            .AsNoTracking()
            .Join(
                _context.Vendas.AsNoTracking(),
                item => item.VendaId,
                venda => venda.Id,
                (item, venda) => new { Item = item, Venda = venda })
            .Where(par => par.Venda.EventoId == eventoId)
            .Select(par => par.Item)
            .ToListAsync(cancellationToken);
}
