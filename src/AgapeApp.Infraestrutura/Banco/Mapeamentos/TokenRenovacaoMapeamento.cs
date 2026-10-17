using AgapeApp.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infraestrutura.Banco.Mapeamentos;

/// <summary>
/// Mapeamento da entidade <see cref="TokenRenovacao"/> para a tabela "tokens_renovacao".
/// </summary>
public class TokenRenovacaoMapeamento : IEntityTypeConfiguration<TokenRenovacao>
{
    public void Configure(EntityTypeBuilder<TokenRenovacao> construtor)
    {
        construtor.ToTable("tokens_renovacao");

        construtor.HasKey(token => token.Id);

        construtor.Property(token => token.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        construtor.Property(token => token.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        construtor.Property(token => token.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(100)
            .IsRequired();

        construtor.HasIndex(token => token.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_tokens_renovacao_token_hash");

        construtor.Property(token => token.ExpiraEm)
            .HasColumnName("expira_em")
            .IsRequired();

        construtor.Property(token => token.RevogadoEm)
            .HasColumnName("revogado_em");

        construtor.Property(token => token.Dispositivo)
            .HasColumnName("dispositivo")
            .HasMaxLength(200);

        construtor.Property(token => token.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        construtor.Property(token => token.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(token => token.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_tokens_renovacao_usuario_id");
    }
}
