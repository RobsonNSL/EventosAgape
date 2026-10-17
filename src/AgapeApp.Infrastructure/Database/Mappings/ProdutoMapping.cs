using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="Produto"/> para a tabela "produtos".
/// </summary>
public class ProdutoMapping : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");

        builder.HasKey(produto => produto.Id);

        builder.Property(produto => produto.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(produto => produto.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(produto => produto.PrecoPadrao)
            .HasColumnName("preco_padrao")
            .HasPrecision(18, 2);

        builder.Property(produto => produto.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(produto => produto.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(produto => produto.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
