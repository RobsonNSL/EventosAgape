using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class SetorRepository : ISetorRepository
{
    private readonly AgapeDbContext _context;

    public SetorRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(Setor setor) => _context.Setores.Add(setor);

    public Task<Setor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Setores
            .AsNoTracking()
            .FirstOrDefaultAsync(setor => setor.Id == id, cancellationToken);

    // Sem AsNoTracking: a entidade é alterada em memória e persistida na unidade de trabalho.
    public Task<Setor?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Setores
            .FirstOrDefaultAsync(setor => setor.Id == id, cancellationToken);

    public Task<bool> ExisteNoEventoAsync(
        Guid eventoId,
        string nome,
        Guid? ignorarSetorId = null,
        CancellationToken cancellationToken = default)
    {
        var nomeNormalizado = nome.Trim();

        return _context.Setores
            .AsNoTracking()
            .AnyAsync(
                setor => setor.EventoId == eventoId
                    && setor.Nome == nomeNormalizado
                    && (!ignorarSetorId.HasValue || setor.Id != ignorarSetorId.Value),
                cancellationToken);
    }

    public async Task<IReadOnlyList<Setor>> ListarPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
        => await _context.Setores
            .AsNoTracking()
            .Where(setor => setor.EventoId == eventoId)
            .OrderBy(setor => setor.Nome)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Setor>> ObterPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();

        if (lista.Count == 0)
            return [];

        return await _context.Setores
            .AsNoTracking()
            .Where(setor => lista.Contains(setor.Id))
            .ToListAsync(cancellationToken);
    }
}
