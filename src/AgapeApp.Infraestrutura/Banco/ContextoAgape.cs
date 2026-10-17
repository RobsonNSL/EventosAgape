using AgapeApp.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infraestrutura.Banco;

/// <summary>
/// Contexto do Entity Framework Core para o banco PostgreSQL do AgapeApp.
/// O schema do banco é gerenciado pelas migrations do FluentMigrator.
/// </summary>
public class ContextoAgape : DbContext
{
    public ContextoAgape(DbContextOptions<ContextoAgape> opcoes) : base(opcoes)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<TokenRenovacao> TokensRenovacao => Set<TokenRenovacao>();

    protected override void OnModelCreating(ModelBuilder construtor)
    {
        construtor.ApplyConfigurationsFromAssembly(typeof(ContextoAgape).Assembly);
    }
}
