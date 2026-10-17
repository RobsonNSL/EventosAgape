using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela evento_produtos: preço praticado no evento e controle de estoque opcional.
/// Único por (evento, produto).
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(7, "Cria a tabela de produtos do evento")]
public class Migration0007CriarTabelaEventoProdutos : Migration
{
    public override void Up()
    {
        Create.Table("evento_produtos")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("evento_id").AsGuid().NotNullable()
            .WithColumn("produto_id").AsGuid().NotNullable()
            .WithColumn("preco").AsDecimal(18, 2).NotNullable()
            .WithColumn("controla_estoque").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("quantidade_estoque").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ux_evento_produtos_evento_produto")
            .OnTable("evento_produtos")
            .OnColumn("evento_id").Ascending()
            .OnColumn("produto_id").Ascending()
            .WithOptions().Unique();

        Create.ForeignKey("fk_evento_produtos_evento_id")
            .FromTable("evento_produtos").ForeignColumn("evento_id")
            .ToTable("eventos").PrimaryColumn("id");

        Create.ForeignKey("fk_evento_produtos_produto_id")
            .FromTable("evento_produtos").ForeignColumn("produto_id")
            .ToTable("produtos").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
