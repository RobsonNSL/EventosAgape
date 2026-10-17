using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="SaldoCartaoEvento"/> para a tabela "saldos_cartao_evento".
/// </summary>
public class SaldoCartaoEventoMapping : IEntityTypeConfiguration<SaldoCartaoEvento>
{
    public void Configure(EntityTypeBuilder<SaldoCartaoEvento> builder)
    {
        builder.ToTable("saldos_cartao_evento");

        builder.HasKey(saldo => saldo.Id);

        builder.Property(saldo => saldo.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(saldo => saldo.CartaoId)
            .HasColumnName("cartao_id")
            .IsRequired();

        builder.Property(saldo => saldo.EventoId)
            .HasColumnName("evento_id")
            .IsRequired();

        builder.Property(saldo => saldo.Saldo)
            .HasColumnName("saldo")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(saldo => new { saldo.CartaoId, saldo.EventoId })
            .IsUnique()
            .HasDatabaseName("ux_saldos_cartao_evento_cartao_evento");

        builder.Property(saldo => saldo.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(saldo => saldo.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
