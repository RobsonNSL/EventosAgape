using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de eventos (ciclo Rascunho → Aberto → Encerrado).
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(5, "Cria a tabela de eventos")]
public class Migration0005CriarTabelaEventos : Migration
{
    public override void Up()
    {
        Create.Table("eventos")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("nome").AsString(150).NotNullable()
            .WithColumn("data_inicio").AsDate().NotNullable()
            .WithColumn("data_fim").AsDate().Nullable()
            .WithColumn("situacao").AsString(20).NotNullable()
            .WithColumn("criado_por").AsGuid().NotNullable()
            .WithColumn("criado_em").AsDateTimeOffset().NotNullable()
            .WithColumn("atualizado_em").AsDateTimeOffset().NotNullable();

        Create.ForeignKey("fk_eventos_criado_por")
            .FromTable("eventos").ForeignColumn("criado_por")
            .ToTable("usuarios").PrimaryColumn("id");
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
