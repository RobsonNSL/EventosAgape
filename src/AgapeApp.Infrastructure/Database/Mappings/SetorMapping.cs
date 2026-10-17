using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="Setor"/> para a tabela "setores".
/// </summary>
public class SetorMapping : IEntityTypeConfiguration<Setor>
{
    public void Configure(EntityTypeBuilder<Setor> builder)
    {
        builder.ToTable("setores");

        builder.HasKey(setor => setor.Id);

        builder.Property(setor => setor.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(setor => setor.EventoId)
            .HasColumnName("evento_id")
            .IsRequired();

        builder.Property(setor => setor.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(setor => new { setor.EventoId, setor.Nome })
            .IsUnique()
            .HasDatabaseName("ux_setores_evento_nome");

        builder.Property(setor => setor.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(setor => setor.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
