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

    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<Cartao> Cartoes => Set<Cartao>();

    public DbSet<Evento> Eventos => Set<Evento>();

    public DbSet<Setor> Setores => Set<Setor>();

    public DbSet<EventoProduto> EventoProdutos => Set<EventoProduto>();

    public DbSet<SetorProduto> SetorProdutos => Set<SetorProduto>();

    public DbSet<EventoOperador> EventoOperadores => Set<EventoOperador>();

    public DbSet<MovimentoEstoque> MovimentosEstoque => Set<MovimentoEstoque>();

    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    public DbSet<SaldoCartaoEvento> SaldosCartaoEvento => Set<SaldoCartaoEvento>();

    public DbSet<Recarga> Recargas => Set<Recarga>();

    public DbSet<Venda> Vendas => Set<Venda>();

    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AgapeDbContext).Assembly);
    }
}
