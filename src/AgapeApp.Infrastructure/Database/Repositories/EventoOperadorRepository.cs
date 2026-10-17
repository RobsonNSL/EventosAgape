using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class EventoOperadorRepository : IEventoOperadorRepository
{
    private readonly AgapeDbContext _context;

    public EventoOperadorRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(EventoOperador designacao) => _context.EventoOperadores.Add(designacao);

    public Task<EventoOperador?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.EventoOperadores
            .AsNoTracking()
            .FirstOrDefaultAsync(designacao => designacao.Id == id, cancellationToken);

    // Sem AsNoTracking: a entidade é alterada em memória e persistida na unidade de trabalho.
    public Task<EventoOperador?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.EventoOperadores
            .FirstOrDefaultAsync(designacao => designacao.Id == id, cancellationToken);

    public Task<EventoOperador?> ObterPorEventoEUsuarioAsync(
        Guid eventoId,
        Guid usuarioId,
        CancellationToken cancellationToken = default)
        => _context.EventoOperadores
            .AsNoTracking()
            .FirstOrDefaultAsync(
                designacao => designacao.EventoId == eventoId && designacao.UsuarioId == usuarioId,
                cancellationToken);

    public async Task<IReadOnlyList<EventoOperador>> ListarPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => await _context.EventoOperadores
            .AsNoTracking()
            .Where(designacao => designacao.EventoId == eventoId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EventoOperador>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
        => await _context.EventoOperadores
            .AsNoTracking()
            .Where(designacao => designacao.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
}
