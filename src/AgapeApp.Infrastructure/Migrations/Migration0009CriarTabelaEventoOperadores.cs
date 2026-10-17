using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela evento_operadores: designação de operadores aos setores do evento.
/// Único por (evento, usuário).
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(9, "Cria a tabela de operadores do evento")]
public class Migration0009CriarTabelaEventoOperadores : Migration
{
    public override void Up()
    {
        Create.Table("evento_operadores")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("evento_id").AsGuid().NotNullable()
            .WithColumn("usuario_id").AsGuid().NotNullable()
            .WithColumn("setor_id").AsGuid().NotNullable()
            .WithColumn("habilitado").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ux_evento_operadores_evento_usuario")
            .OnTable("evento_operadores")
            .OnColumn("evento_id").Ascending()
            .OnColumn("usuario_id").Ascending()
            .WithOptions().Unique();

        Create.ForeignKey("fk_evento_operadores_evento_id")
            .FromTable("evento_operadores").ForeignColumn("evento_id")
            .ToTable("eventos").PrimaryColumn("id");

        Create.ForeignKey("fk_evento_operadores_usuario_id")
            .FromTable("evento_operadores").ForeignColumn("usuario_id")
            .ToTable("usuarios").PrimaryColumn("id");

        Create.ForeignKey("fk_evento_operadores_setor_id")
            .FromTable("evento_operadores").ForeignColumn("setor_id")
            .ToTable("setores").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
