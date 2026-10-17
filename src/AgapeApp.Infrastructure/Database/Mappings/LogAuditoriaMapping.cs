using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="LogAuditoria"/> para a tabela "logs_auditoria".
/// </summary>
public class LogAuditoriaMapping : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("logs_auditoria");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(log => log.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(log => log.Acao)
            .HasColumnName("acao")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.Entidade)
            .HasColumnName("entidade")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.EntidadeId)
            .HasColumnName("entidade_id")
            .IsRequired();

        builder.Property(log => log.EventoId)
            .HasColumnName("evento_id");

        builder.Property(log => log.Dados)
            .HasColumnName("dados");

        builder.Property(log => log.Dispositivo)
            .HasColumnName("dispositivo")
            .HasMaxLength(100);

        builder.Property(log => log.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(log => log.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
