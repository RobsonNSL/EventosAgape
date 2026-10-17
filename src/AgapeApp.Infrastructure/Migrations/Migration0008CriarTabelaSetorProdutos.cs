using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela setor_produtos: define o que cada setor vende.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(8, "Cria a tabela de produtos do setor")]
public class Migration0008CriarTabelaSetorProdutos : Migration
{
    public override void Up()
    {
        Create.Table("setor_produtos")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("setor_id").AsGuid().NotNullable()
            .WithColumn("evento_produto_id").AsGuid().NotNullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ux_setor_produtos_setor_evento_produto")
            .OnTable("setor_produtos")
            .OnColumn("setor_id").Ascending()
            .OnColumn("evento_produto_id").Ascending()
            .WithOptions().Unique();

        Create.ForeignKey("fk_setor_produtos_setor_id")
            .FromTable("setor_produtos").ForeignColumn("setor_id")
            .ToTable("setores").PrimaryColumn("id");

        Create.ForeignKey("fk_setor_produtos_evento_produto_id")
            .FromTable("setor_produtos").ForeignColumn("evento_produto_id")
            .ToTable("evento_produtos").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
