using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class SaldoCartaoEventoRepository : ISaldoCartaoEventoRepository
{
    private readonly AgapeDbContext _context;

    public SaldoCartaoEventoRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(SaldoCartaoEvento saldo) => _context.SaldosCartaoEvento.Add(saldo);

    // Sem AsNoTracking: o saldo é creditado/debitado em memória e persistido na unidade de trabalho.
    public Task<SaldoCartaoEvento?> ObterAsync(
        Guid cartaoId,
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => _context.SaldosCartaoEvento
            .FirstOrDefaultAsync(
                saldo => saldo.CartaoId == cartaoId && saldo.EventoId == eventoId,
                cancellationToken);

    public async Task<IReadOnlyList<SaldoCartaoEvento>> ListarPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => await _context.SaldosCartaoEvento
            .AsNoTracking()
            .Where(saldo => saldo.EventoId == eventoId && saldo.Saldo > 0)
            .ToListAsync(cancellationToken);
}
