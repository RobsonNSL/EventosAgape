using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class EventoRepository : IEventoRepository
{
    private readonly AgapeDbContext _context;

    public EventoRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(Evento evento) => _context.Eventos.Add(evento);

    public Task<Evento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(evento => evento.Id == id, cancellationToken);

    // Sem AsNoTracking: a entidade é alterada em memória e persistida na unidade de trabalho.
    public Task<Evento?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Eventos
            .FirstOrDefaultAsync(evento => evento.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Evento>> ListarAsync(
        SituacaoEvento? situacao = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Eventos.AsNoTracking();

        if (situacao.HasValue)
            consulta = consulta.Where(evento => evento.Situacao == situacao.Value);

        return await consulta
            .OrderByDescending(evento => evento.DataInicio)
            .ThenBy(evento => evento.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Evento>> ObterPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();

        if (lista.Count == 0)
            return [];

        return await _context.Eventos
            .AsNoTracking()
            .Where(evento => lista.Contains(evento.Id))
            .ToListAsync(cancellationToken);
    }
}
