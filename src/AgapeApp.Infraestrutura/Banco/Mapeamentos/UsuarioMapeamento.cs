using AgapeApp.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infraestrutura.Banco.Mapeamentos;

/// <summary>
/// Mapeamento da entidade <see cref="Usuario"/> para a tabela "usuarios".
/// </summary>
public class UsuarioMapeamento : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> construtor)
    {
        construtor.ToTable("usuarios");

        construtor.HasKey(usuario => usuario.Id);

        construtor.Property(usuario => usuario.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        construtor.Property(usuario => usuario.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        construtor.Property(usuario => usuario.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        construtor.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasDatabaseName("ix_usuarios_email");

        construtor.Property(usuario => usuario.SenhaHash)
            .HasColumnName("senha_hash")
            .HasMaxLength(500)
            .IsRequired();

        construtor.Property(usuario => usuario.Perfil)
            .HasColumnName("perfil")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        construtor.Property(usuario => usuario.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        construtor.Property(usuario => usuario.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        construtor.Property(usuario => usuario.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
