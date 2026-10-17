using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="Recarga"/> para a tabela "recargas".
/// </summary>
public class RecargaMapping : IEntityTypeConfiguration<Recarga>
{
    public void Configure(EntityTypeBuilder<Recarga> builder)
    {
        builder.ToTable("recargas");

        builder.HasKey(recarga => recarga.Id);

        builder.Property(recarga => recarga.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(recarga => recarga.CartaoId)
            .HasColumnName("cartao_id")
            .IsRequired();

        builder.Property(recarga => recarga.EventoId)
            .HasColumnName("evento_id")
            .IsRequired();

        builder.Property(recarga => recarga.SetorId)
            .HasColumnName("setor_id")
            .IsRequired();

        builder.Property(recarga => recarga.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(recarga => recarga.Valor)
            .HasColumnName("valor")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(recarga => recarga.IdOperacaoCliente)
            .HasColumnName("id_operacao_cliente")
            .IsRequired();

        builder.HasIndex(recarga => recarga.IdOperacaoCliente)
            .IsUnique()
            .HasDatabaseName("ux_recargas_id_operacao_cliente");

        builder.Property(recarga => recarga.Dispositivo)
            .HasColumnName("dispositivo")
            .HasMaxLength(100);

        builder.Property(recarga => recarga.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(recarga => recarga.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
