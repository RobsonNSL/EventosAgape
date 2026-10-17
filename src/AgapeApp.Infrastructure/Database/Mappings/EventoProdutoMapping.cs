using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="EventoProduto"/> para a tabela "evento_produtos".
/// </summary>
public class EventoProdutoMapping : IEntityTypeConfiguration<EventoProduto>
{
    public void Configure(EntityTypeBuilder<EventoProduto> builder)
    {
        builder.ToTable("evento_produtos");

        builder.HasKey(eventoProduto => eventoProduto.Id);

        builder.Property(eventoProduto => eventoProduto.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(eventoProduto => eventoProduto.EventoId)
            .HasColumnName("evento_id")
            .IsRequired();

        builder.Property(eventoProduto => eventoProduto.ProdutoId)
            .HasColumnName("produto_id")
            .IsRequired();

        builder.Property(eventoProduto => eventoProduto.Preco)
            .HasColumnName("preco")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(eventoProduto => eventoProduto.ControlaEstoque)
            .HasColumnName("controla_estoque")
            .IsRequired();

        builder.Property(eventoProduto => eventoProduto.QuantidadeEstoque)
            .HasColumnName("quantidade_estoque")
            .IsRequired();

        builder.HasIndex(eventoProduto => new { eventoProduto.EventoId, eventoProduto.ProdutoId })
            .IsUnique()
            .HasDatabaseName("ux_evento_produtos_evento_produto");

        builder.Property(eventoProduto => eventoProduto.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(eventoProduto => eventoProduto.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
