using FluentMigrator;

namespace AgapeApp.Infraestrutura.Migracoes;

/// <summary>
/// Cria a tabela de usuários.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(1, "Cria a tabela de usuários")]
public class Migracao0001CriarTabelaUsuarios : Migration
{
    public override void Up()
    {
        Create.Table("usuarios")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("nome").AsString(150).NotNullable()
            .WithColumn("email").AsString(200).NotNullable()
            .WithColumn("senha_hash").AsString(500).NotNullable()
            .WithColumn("perfil").AsString(20).NotNullable()
            .WithColumn("ativo").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("criado_em").AsCustom("timestamptz").NotNullable()
            .WithColumn("atualizado_em").AsCustom("timestamptz").NotNullable();

        Create.Index("ix_usuarios_email")
            .OnTable("usuarios")
            .OnColumn("email").Ascending()
            .WithOptions().Unique();
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
