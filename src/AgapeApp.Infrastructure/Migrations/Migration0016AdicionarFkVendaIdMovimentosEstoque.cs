using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Adiciona a chave estrangeira de movimentos_estoque.venda_id → vendas.id.
/// O vínculo entra depois da criação da tabela de vendas (a coluna já existia desde a 0010).
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(16, "Adiciona a chave estrangeira da venda nos movimentos de estoque")]
public class Migration0016AdicionarFkVendaIdMovimentosEstoque : Migration
{
    public override void Up()
    {
        Create.ForeignKey("fk_movimentos_estoque_venda_id")
            .FromTable("movimentos_estoque").ForeignColumn("venda_id")
            .ToTable("vendas").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
