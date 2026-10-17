using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de registros de auditoria (quem, o quê, quando, dispositivo).
/// A coluna evento_id é uma adição deliberada ao plano original: permite consultar a auditoria por evento.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(11, "Cria a tabela de registros de auditoria")]
public class Migration0011CriarTabelaLogsAuditoria : Migration
{
    public override void Up()
    {
        Create.Table("logs_auditoria")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("usuario_id").AsGuid().NotNullable()
            .WithColumn("acao").AsString(100).NotNullable()
            .WithColumn("entidade").AsString(100).NotNullable()
            .WithColumn("entidade_id").AsGuid().NotNullable()
            .WithColumn("evento_id").AsGuid().Nullable()
            .WithColumn("dados").AsString(int.MaxValue).Nullable()
            .WithColumn("dispositivo").AsString(100).Nullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ix_logs_auditoria_evento_id")
            .OnTable("logs_auditoria")
            .OnColumn("evento_id").Ascending();

        Create.Index("ix_logs_auditoria_entidade_entidade_id")
            .OnTable("logs_auditoria")
            .OnColumn("entidade").Ascending()
            .OnColumn("entidade_id").Ascending();

        Create.ForeignKey("fk_logs_auditoria_usuario_id")
            .FromTable("logs_auditoria").ForeignColumn("usuario_id")
            .ToTable("usuarios").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
