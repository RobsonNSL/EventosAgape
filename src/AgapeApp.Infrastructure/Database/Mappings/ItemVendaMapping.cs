using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="ItemVenda"/> para a tabela "itens_venda".
/// </summary>
public class ItemVendaMapping : IEntityTypeConfiguration<ItemVenda>
{
    public void Configure(EntityTypeBuilder<ItemVenda> builder)
    {
        builder.ToTable("itens_venda");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.VendaId)
            .HasColumnName("venda_id")
            .IsRequired();

        builder.Property(item => item.EventoProdutoId)
            .HasColumnName("evento_produto_id")
            .IsRequired();

        builder.Property(item => item.ProdutoNome)
            .HasColumnName("produto_nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(item => item.PrecoUnitario)
            .HasColumnName("preco_unitario")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.Quantidade)
            .HasColumnName("quantidade")
            .IsRequired();

        builder.Property(item => item.Total)
            .HasColumnName("total")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(item => item.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();

        // Relações com o banco espelhando as chaves estrangeiras das migrations.
        // Além da integridade, o EF usa as relações para ordenar os INSERTs da unidade de trabalho
        // (a venda precisa existir antes dos itens).
        builder.HasOne<Venda>()
            .WithMany()
            .HasForeignKey(item => item.VendaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EventoProduto>()
            .WithMany()
            .HasForeignKey(item => item.EventoProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
