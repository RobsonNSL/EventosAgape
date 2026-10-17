using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database;

/// <summary>
/// Contexto do Entity Framework Core para o banco PostgreSQL do AgapeApp.
/// O schema do banco é gerenciado pelas migrations do FluentMigrator.
/// </summary>
public class AgapeDbContext : DbContext
{
    public AgapeDbContext(DbContextOptions<AgapeDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<TokenRenovacao> TokensRenovacao => Set<TokenRenovacao>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AgapeDbContext).Assembly);
    }
}
