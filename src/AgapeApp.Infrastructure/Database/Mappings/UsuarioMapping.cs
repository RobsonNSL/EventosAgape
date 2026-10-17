using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="Usuario"/> para a tabela "usuarios".
/// </summary>
public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(usuario => usuario.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(usuario => usuario.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasDatabaseName("ix_usuarios_email");

        builder.Property(usuario => usuario.SenhaHash)
            .HasColumnName("senha_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(usuario => usuario.Perfil)
            .HasColumnName("perfil")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(usuario => usuario.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(usuario => usuario.CaminhoFoto)
            .HasColumnName("caminho_foto")
            .HasMaxLength(300);

        builder.Property(usuario => usuario.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(usuario => usuario.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
