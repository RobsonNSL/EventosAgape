using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class RecargaRepository : IRecargaRepository
{
    private readonly AgapeDbContext _context;

    public RecargaRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(Recarga recarga) => _context.Recargas.Add(recarga);

    public Task<Recarga?> ObterPorIdOperacaoAsync(
        Guid idOperacaoCliente,
        CancellationToken cancellationToken = default)
        => _context.Recargas
            .AsNoTracking()
            .FirstOrDefaultAsync(recarga => recarga.IdOperacaoCliente == idOperacaoCliente, cancellationToken);

    public async Task<IReadOnlyList<Recarga>> ListarPorEventoAsync(
        Guid eventoId,
        int pular,
        int quantidade,
        CancellationToken cancellationToken = default)
        => await _context.Recargas
            .AsNoTracking()
            .Where(recarga => recarga.EventoId == eventoId)
            .OrderByDescending(recarga => recarga.CriadoEm)
            .Skip(pular)
            .Take(quantidade)
            .ToListAsync(cancellationToken);

    public Task<int> ContarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
        => _context.Recargas
            .AsNoTracking()
            .CountAsync(recarga => recarga.EventoId == eventoId, cancellationToken);

    public async Task<IReadOnlyList<Recarga>> ListarTodasPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => await _context.Recargas
            .AsNoTracking()
            .Where(recarga => recarga.EventoId == eventoId)
            .OrderBy(recarga => recarga.CriadoEm)
            .ToListAsync(cancellationToken);
}
