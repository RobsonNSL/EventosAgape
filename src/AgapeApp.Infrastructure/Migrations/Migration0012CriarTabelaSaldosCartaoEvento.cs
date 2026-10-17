using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de saldos de cartão por evento (criado na primeira recarga; nunca atravessa eventos).
/// Único por (cartão, evento).
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(12, "Cria a tabela de saldos de cartão por evento")]
public class Migration0012CriarTabelaSaldosCartaoEvento : Migration
{
    public override void Up()
    {
        Create.Table("saldos_cartao_evento")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("cartao_id").AsGuid().NotNullable()
            .WithColumn("evento_id").AsGuid().NotNullable()
            .WithColumn("saldo").AsDecimal(18, 2).NotNullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ux_saldos_cartao_evento_cartao_evento")
            .OnTable("saldos_cartao_evento")
            .OnColumn("cartao_id").Ascending()
            .OnColumn("evento_id").Ascending()
            .WithOptions().Unique();

        Create.ForeignKey("fk_saldos_cartao_evento_cartao_id")
            .FromTable("saldos_cartao_evento").ForeignColumn("cartao_id")
            .ToTable("cartoes").PrimaryColumn("id");

        Create.ForeignKey("fk_saldos_cartao_evento_evento_id")
            .FromTable("saldos_cartao_evento").ForeignColumn("evento_id")
            .ToTable("eventos").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
