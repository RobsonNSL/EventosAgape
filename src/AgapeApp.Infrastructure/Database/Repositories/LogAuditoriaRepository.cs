using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class LogAuditoriaRepository : ILogAuditoriaRepository
{
    private readonly AgapeDbContext _context;

    public LogAuditoriaRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(LogAuditoria log) => _context.LogsAuditoria.Add(log);

    public async Task<IReadOnlyList<LogAuditoria>> ListarPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => await _context.LogsAuditoria
            .AsNoTracking()
            .Where(log => log.EventoId == eventoId)
            .OrderByDescending(log => log.CriadoEm)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LogAuditoria>> ListarPorEventoAsync(
        Guid eventoId,
        int pular,
        int quantidade,
        CancellationToken cancellationToken = default)
        => await _context.LogsAuditoria
            .AsNoTracking()
            .Where(log => log.EventoId == eventoId)
            .OrderByDescending(log => log.CriadoEm)
            .Skip(pular)
            .Take(quantidade)
            .ToListAsync(cancellationToken);

    public Task<int> ContarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
        => _context.LogsAuditoria
            .AsNoTracking()
            .CountAsync(log => log.EventoId == eventoId, cancellationToken);
}
