using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de recargas. O índice único em id_operacao_cliente garante a idempotência
/// (retry ou toque duplo do app nunca duplica a recarga).
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(13, "Cria a tabela de recargas")]
public class Migration0013CriarTabelaRecargas : Migration
{
    public override void Up()
    {
        Create.Table("recargas")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("cartao_id").AsGuid().NotNullable()
            .WithColumn("evento_id").AsGuid().NotNullable()
            .WithColumn("setor_id").AsGuid().NotNullable()
            .WithColumn("usuario_id").AsGuid().NotNullable()
            .WithColumn("valor").AsDecimal(18, 2).NotNullable()
            .WithColumn("id_operacao_cliente").AsGuid().NotNullable()
            .WithColumn("dispositivo").AsString(100).Nullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ux_recargas_id_operacao_cliente")
            .OnTable("recargas")
            .OnColumn("id_operacao_cliente").Ascending()
            .WithOptions().Unique();

        Create.Index("ix_recargas_evento_id")
            .OnTable("recargas")
            .OnColumn("evento_id").Ascending();

        Create.ForeignKey("fk_recargas_cartao_id")
            .FromTable("recargas").ForeignColumn("cartao_id")
            .ToTable("cartoes").PrimaryColumn("id");

        Create.ForeignKey("fk_recargas_evento_id")
            .FromTable("recargas").ForeignColumn("evento_id")
            .ToTable("eventos").PrimaryColumn("id");

        Create.ForeignKey("fk_recargas_setor_id")
            .FromTable("recargas").ForeignColumn("setor_id")
            .ToTable("setores").PrimaryColumn("id");

        Create.ForeignKey("fk_recargas_usuario_id")
            .FromTable("recargas").ForeignColumn("usuario_id")
            .ToTable("usuarios").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
