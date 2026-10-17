using AgapeApp.Application.Interfaces;

namespace AgapeApp.Infrastructure.Database;

public class UnitOfWork : IUnitOfWork
{
    private readonly AgapeDbContext _context;

    public UnitOfWork(AgapeDbContext context)
    {
        _context = context;
    }

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
