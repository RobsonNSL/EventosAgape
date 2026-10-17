using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgapeApp.Infrastructure.Database.Mappings;

/// <summary>
/// Mapeamento da entidade <see cref="TokenRenovacao"/> para a tabela "tokens_renovacao".
/// </summary>
public class TokenRenovacaoMapping : IEntityTypeConfiguration<TokenRenovacao>
{
    public void Configure(EntityTypeBuilder<TokenRenovacao> builder)
    {
        builder.ToTable("tokens_renovacao");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(token => token.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(token => token.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(token => token.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_tokens_renovacao_token_hash");

        builder.Property(token => token.ExpiraEm)
            .HasColumnName("expira_em")
            .IsRequired();

        builder.Property(token => token.RevogadoEm)
            .HasColumnName("revogado_em");

        builder.Property(token => token.Dispositivo)
            .HasColumnName("dispositivo")
            .HasMaxLength(200);

        builder.Property(token => token.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(token => token.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(token => token.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_tokens_renovacao_usuario_id");
    }
}
