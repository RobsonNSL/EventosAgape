using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="Cartao"/> para a tabela "cartoes".
/// </summary>
public class CartaoMapping : IEntityTypeConfiguration<Cartao>
{
    public void Configure(EntityTypeBuilder<Cartao> builder)
    {
        builder.ToTable("cartoes");

        builder.HasKey(cartao => cartao.Id);

        builder.Property(cartao => cartao.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(cartao => cartao.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(cartao => cartao.Codigo)
            .IsUnique()
            .HasDatabaseName("ix_cartoes_codigo");

        builder.Property(cartao => cartao.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(cartao => cartao.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(cartao => cartao.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
