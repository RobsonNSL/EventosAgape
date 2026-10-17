using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela movimentos_estoque: toda movimentação de estoque (estoque inicial,
/// venda e ajuste manual). O vínculo com vendas (venda_id) é criado junto da tabela de vendas.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(10, "Cria a tabela de movimentos de estoque")]
public class Migration0010CriarTabelaMovimentosEstoque : Migration
{
    public override void Up()
    {
        Create.Table("movimentos_estoque")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("evento_produto_id").AsGuid().NotNullable()
            .WithColumn("quantidade").AsInt32().NotNullable()
            .WithColumn("motivo").AsString(20).NotNullable()
            .WithColumn("venda_id").AsGuid().Nullable()
            .WithColumn("usuario_id").AsGuid().NotNullable()
            .WithColumn("observacao").AsString(300).Nullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ix_movimentos_estoque_evento_produto_id")
            .OnTable("movimentos_estoque")
            .OnColumn("evento_produto_id").Ascending();

        Create.Index("ix_movimentos_estoque_venda_id")
            .OnTable("movimentos_estoque")
            .OnColumn("venda_id").Ascending();

        Create.ForeignKey("fk_movimentos_estoque_evento_produto_id")
            .FromTable("movimentos_estoque").ForeignColumn("evento_produto_id")
            .ToTable("evento_produtos").PrimaryColumn("id");

        Create.ForeignKey("fk_movimentos_estoque_usuario_id")
            .FromTable("movimentos_estoque").ForeignColumn("usuario_id")
            .ToTable("usuarios").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
