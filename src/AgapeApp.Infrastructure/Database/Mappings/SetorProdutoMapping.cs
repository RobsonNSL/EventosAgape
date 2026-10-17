using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="SetorProduto"/> para a tabela "setor_produtos".
/// </summary>
public class SetorProdutoMapping : IEntityTypeConfiguration<SetorProduto>
{
    public void Configure(EntityTypeBuilder<SetorProduto> builder)
    {
        builder.ToTable("setor_produtos");

        builder.HasKey(setorProduto => setorProduto.Id);

        builder.Property(setorProduto => setorProduto.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(setorProduto => setorProduto.SetorId)
            .HasColumnName("setor_id")
            .IsRequired();

        builder.Property(setorProduto => setorProduto.EventoProdutoId)
            .HasColumnName("evento_produto_id")
            .IsRequired();

        builder.HasIndex(setorProduto => new { setorProduto.SetorId, setorProduto.EventoProdutoId })
            .IsUnique()
            .HasDatabaseName("ux_setor_produtos_setor_evento_produto");

        builder.Property(setorProduto => setorProduto.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(setorProduto => setorProduto.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
