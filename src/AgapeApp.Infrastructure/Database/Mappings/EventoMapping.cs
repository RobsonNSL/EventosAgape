using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="Evento"/> para a tabela "eventos".
/// </summary>
public class EventoMapping : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("eventos");

        builder.HasKey(evento => evento.Id);

        builder.Property(evento => evento.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(evento => evento.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(evento => evento.DataInicio)
            .HasColumnName("data_inicio")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(evento => evento.DataFim)
            .HasColumnName("data_fim")
            .HasColumnType("date");

        builder.Property(evento => evento.Situacao)
            .HasColumnName("situacao")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(evento => evento.CriadoPor)
            .HasColumnName("criado_por")
            .IsRequired();

        builder.Property(evento => evento.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(evento => evento.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
