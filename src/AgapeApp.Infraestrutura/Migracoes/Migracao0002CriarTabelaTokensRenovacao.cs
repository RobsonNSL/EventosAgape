using System.Data;
using FluentMigrator;

namespace AgapeApp.Infraestrutura.Migracoes;

/// <summary>
/// Cria a tabela de tokens de renovação (refresh tokens) com vínculo ao usuário.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(2, "Cria a tabela de tokens de renovação")]
public class Migracao0002CriarTabelaTokensRenovacao : Migration
{
    public override void Up()
    {
        Create.Table("tokens_renovacao")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("usuario_id").AsGuid().NotNullable()
            .WithColumn("token_hash").AsString(100).NotNullable()
            .WithColumn("expira_em").AsCustom("timestamptz").NotNullable()
            .WithColumn("revogado_em").AsCustom("timestamptz").Nullable()
            .WithColumn("dispositivo").AsString(200).Nullable()
            .WithColumn("criado_em").AsCustom("timestamptz").NotNullable()
            .WithColumn("atualizado_em").AsCustom("timestamptz").NotNullable();

        Create.Index("ix_tokens_renovacao_token_hash")
            .OnTable("tokens_renovacao")
            .OnColumn("token_hash").Ascending()
            .WithOptions().Unique();

        Create.ForeignKey("fk_tokens_renovacao_usuario_id")
            .FromTable("tokens_renovacao").ForeignColumn("usuario_id")
            .ToTable("usuarios").PrimaryColumn("id")
            .OnDelete(Rule.Cascade);
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
