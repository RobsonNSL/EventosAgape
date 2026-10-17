using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="MovimentoEstoque"/> para a tabela "movimentos_estoque".
/// </summary>
public class MovimentoEstoqueMapping : IEntityTypeConfiguration<MovimentoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentoEstoque> builder)
    {
        builder.ToTable("movimentos_estoque");

        builder.HasKey(movimento => movimento.Id);

        builder.Property(movimento => movimento.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(movimento => movimento.EventoProdutoId)
            .HasColumnName("evento_produto_id")
            .IsRequired();

        builder.Property(movimento => movimento.Quantidade)
            .HasColumnName("quantidade")
            .IsRequired();

        builder.Property(movimento => movimento.Motivo)
            .HasColumnName("motivo")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(movimento => movimento.VendaId)
            .HasColumnName("venda_id");

        builder.Property(movimento => movimento.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(movimento => movimento.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(300);

        builder.Property(movimento => movimento.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(movimento => movimento.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();

        // Relações com o banco espelhando as chaves estrangeiras das migrations.
        // Além da integridade, o EF usa as relações para ordenar os INSERTs da unidade de trabalho
        // (o vínculo do produto e a venda precisam existir antes do movimento).
        builder.HasOne<EventoProduto>()
            .WithMany()
            .HasForeignKey(movimento => movimento.EventoProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Venda>()
            .WithMany()
            .HasForeignKey(movimento => movimento.VendaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
