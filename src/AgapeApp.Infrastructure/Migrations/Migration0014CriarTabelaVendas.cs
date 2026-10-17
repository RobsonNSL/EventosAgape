using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de vendas. O índice único em id_operacao_cliente garante a idempotência.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(14, "Cria a tabela de vendas")]
public class Migration0014CriarTabelaVendas : Migration
{
    public override void Up()
    {
        Create.Table("vendas")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("evento_id").AsGuid().NotNullable()
            .WithColumn("cartao_id").AsGuid().NotNullable()
            .WithColumn("setor_id").AsGuid().NotNullable()
            .WithColumn("usuario_id").AsGuid().NotNullable()
            .WithColumn("total").AsDecimal(18, 2).NotNullable()
            .WithColumn("id_operacao_cliente").AsGuid().NotNullable()
            .WithColumn("dispositivo").AsString(100).Nullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.Index("ux_vendas_id_operacao_cliente")
            .OnTable("vendas")
            .OnColumn("id_operacao_cliente").Ascending()
            .WithOptions().Unique();

        Create.Index("ix_vendas_evento_id")
            .OnTable("vendas")
            .OnColumn("evento_id").Ascending();

        Create.ForeignKey("fk_vendas_evento_id")
            .FromTable("vendas").ForeignColumn("evento_id")
            .ToTable("eventos").PrimaryColumn("id");

        Create.ForeignKey("fk_vendas_cartao_id")
            .FromTable("vendas").ForeignColumn("cartao_id")
            .ToTable("cartoes").PrimaryColumn("id");

        Create.ForeignKey("fk_vendas_setor_id")
            .FromTable("vendas").ForeignColumn("setor_id")
            .ToTable("setores").PrimaryColumn("id");

        Create.ForeignKey("fk_vendas_usuario_id")
            .FromTable("vendas").ForeignColumn("usuario_id")
            .ToTable("usuarios").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
