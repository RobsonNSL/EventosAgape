using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de cartões (código único, ex.: CARD-0001).
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(4, "Cria a tabela de cartões")]
public class Migration0004CriarTabelaCartoes : Migration
{
    public override void Up()
    {
        Create.Table("cartoes")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("codigo").AsString(50).NotNullable()
            .WithColumn("ativo").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ix_cartoes_codigo")
            .OnTable("cartoes")
            .OnColumn("codigo").Ascending()
            .WithOptions().Unique();
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
