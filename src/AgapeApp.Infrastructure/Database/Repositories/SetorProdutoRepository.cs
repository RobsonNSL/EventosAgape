using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class SetorProdutoRepository : ISetorProdutoRepository
{
    private readonly AgapeDbContext _context;

    public SetorProdutoRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(SetorProduto setorProduto) => _context.SetorProdutos.Add(setorProduto);

    public void Remover(SetorProduto setorProduto) => _context.SetorProdutos.Remove(setorProduto);

    public Task<SetorProduto?> ObterAsync(
        Guid setorId,
        Guid eventoProdutoId,
        CancellationToken cancellationToken = default)
        => _context.SetorProdutos
            .FirstOrDefaultAsync(
                setorProduto => setorProduto.SetorId == setorId
                    && setorProduto.EventoProdutoId == eventoProdutoId,
                cancellationToken);

    public async Task<IReadOnlyList<SetorProduto>> ListarPorSetorAsync(
        Guid setorId,
        CancellationToken cancellationToken = default)
        => await _context.SetorProdutos
            .AsNoTracking()
            .Where(setorProduto => setorProduto.SetorId == setorId)
            .ToListAsync(cancellationToken);
}
