using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="Venda"/> para a tabela "vendas".
/// </summary>
public class VendaMapping : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("vendas");

        builder.HasKey(venda => venda.Id);

        builder.Property(venda => venda.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(venda => venda.EventoId)
            .HasColumnName("evento_id")
            .IsRequired();

        builder.Property(venda => venda.CartaoId)
            .HasColumnName("cartao_id")
            .IsRequired();

        builder.Property(venda => venda.SetorId)
            .HasColumnName("setor_id")
            .IsRequired();

        builder.Property(venda => venda.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(venda => venda.Total)
            .HasColumnName("total")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(venda => venda.IdOperacaoCliente)
            .HasColumnName("id_operacao_cliente")
            .IsRequired();

        builder.HasIndex(venda => venda.IdOperacaoCliente)
            .IsUnique()
            .HasDatabaseName("ux_vendas_id_operacao_cliente");

        builder.Property(venda => venda.Dispositivo)
            .HasColumnName("dispositivo")
            .HasMaxLength(100);

        builder.Property(venda => venda.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(venda => venda.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
