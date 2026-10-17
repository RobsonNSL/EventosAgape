using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class CartaoRepository : ICartaoRepository
{
    private readonly AgapeDbContext _context;

    public CartaoRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(Cartao cartao) => _context.Cartoes.Add(cartao);

    public Task<Cartao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Cartoes
            .AsNoTracking()
            .FirstOrDefaultAsync(cartao => cartao.Id == id, cancellationToken);

    public Task<Cartao?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => _context.Cartoes
            .AsNoTracking()
            .FirstOrDefaultAsync(cartao => cartao.Codigo == codigo, cancellationToken);

    // Sem AsNoTracking: a entidade é alterada em memória e persistida na unidade de trabalho.
    public Task<Cartao?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Cartoes
            .FirstOrDefaultAsync(cartao => cartao.Id == id, cancellationToken);

    public Task<bool> ExistePorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        => _context.Cartoes
            .AsNoTracking()
            .AnyAsync(cartao => cartao.Codigo == codigo, cancellationToken);

    public async Task<IReadOnlyList<Cartao>> ListarAsync(
        string? codigo,
        bool incluirInativos = false,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Cartoes.AsNoTracking();

        if (!incluirInativos)
            consulta = consulta.Where(cartao => cartao.Ativo);

        if (!string.IsNullOrWhiteSpace(codigo))
        {
            var filtro = Cartao.NormalizarCodigo(codigo);
            consulta = consulta.Where(cartao => cartao.Codigo.Contains(filtro));
        }

        return await consulta
            .OrderBy(cartao => cartao.Codigo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cartao>> ObterPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();

        if (lista.Count == 0)
            return [];

        return await _context.Cartoes
            .AsNoTracking()
            .Where(cartao => lista.Contains(cartao.Id))
            .ToListAsync(cancellationToken);
    }
}
