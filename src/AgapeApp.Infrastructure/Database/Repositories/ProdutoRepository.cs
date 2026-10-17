using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AgapeDbContext _context;

    public ProdutoRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(Produto produto) => _context.Produtos.Add(produto);

    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(produto => produto.Id == id, cancellationToken);

    // Sem AsNoTracking: a entidade é alterada em memória e persistida na unidade de trabalho.
    public Task<Produto?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Produtos
            .FirstOrDefaultAsync(produto => produto.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Produto>> ListarAsync(
        bool incluirInativos = false,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Produtos.AsNoTracking();

        if (!incluirInativos)
            consulta = consulta.Where(produto => produto.Ativo);

        return await consulta
            .OrderBy(produto => produto.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Produto>> ObterPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();

        if (lista.Count == 0)
            return [];

        return await _context.Produtos
            .AsNoTracking()
            .Where(produto => lista.Contains(produto.Id))
            .ToListAsync(cancellationToken);
    }
}
