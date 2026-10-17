using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de itens de venda, com retrato do nome e do preço no momento da venda.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(15, "Cria a tabela de itens de venda")]
public class Migration0015CriarTabelaItensVenda : Migration
{
    public override void Up()
    {
        Create.Table("itens_venda")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("venda_id").AsGuid().NotNullable()
            .WithColumn("evento_produto_id").AsGuid().NotNullable()
            .WithColumn("produto_nome").AsString(150).NotNullable()
            .WithColumn("preco_unitario").AsDecimal(18, 2).NotNullable()
            .WithColumn("quantidade").AsInt32().NotNullable()
            .WithColumn("total").AsDecimal(18, 2).NotNullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ix_itens_venda_venda_id")
            .OnTable("itens_venda")
            .OnColumn("venda_id").Ascending();

        Create.Index("ix_itens_venda_evento_produto_id")
            .OnTable("itens_venda")
            .OnColumn("evento_produto_id").Ascending();

        Create.ForeignKey("fk_itens_venda_venda_id")
            .FromTable("itens_venda").ForeignColumn("venda_id")
            .ToTable("vendas").PrimaryColumn("id");

        Create.ForeignKey("fk_itens_venda_evento_produto_id")
            .FromTable("itens_venda").ForeignColumn("evento_produto_id")
            .ToTable("evento_produtos").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
