using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de setores dos eventos (ex.: Pastel, Bebidas). Nome único por evento.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(6, "Cria a tabela de setores")]
public class Migration0006CriarTabelaSetores : Migration
{
    public override void Up()
    {
        Create.Table("setores")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("evento_id").AsGuid().NotNullable()
            .WithColumn("nome").AsString(100).NotNullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ux_setores_evento_nome")
            .OnTable("setores")
            .OnColumn("evento_id").Ascending()
            .OnColumn("nome").Ascending()
            .WithOptions().Unique();

        Create.ForeignKey("fk_setores_evento_id")
            .FromTable("setores").ForeignColumn("evento_id")
            .ToTable("eventos").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
