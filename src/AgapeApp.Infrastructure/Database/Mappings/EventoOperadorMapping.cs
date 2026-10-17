using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="EventoOperador"/> para a tabela "evento_operadores".
/// </summary>
public class EventoOperadorMapping : IEntityTypeConfiguration<EventoOperador>
{
    public void Configure(EntityTypeBuilder<EventoOperador> builder)
    {
        builder.ToTable("evento_operadores");

        builder.HasKey(eventoOperador => eventoOperador.Id);

        builder.Property(eventoOperador => eventoOperador.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(eventoOperador => eventoOperador.EventoId)
            .HasColumnName("evento_id")
            .IsRequired();

        builder.Property(eventoOperador => eventoOperador.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(eventoOperador => eventoOperador.SetorId)
            .HasColumnName("setor_id")
            .IsRequired();

        builder.Property(eventoOperador => eventoOperador.Habilitado)
            .HasColumnName("habilitado")
            .IsRequired();

        builder.HasIndex(eventoOperador => new { eventoOperador.EventoId, eventoOperador.UsuarioId })
            .IsUnique()
            .HasDatabaseName("ux_evento_operadores_evento_usuario");

        builder.Property(eventoOperador => eventoOperador.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(eventoOperador => eventoOperador.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
