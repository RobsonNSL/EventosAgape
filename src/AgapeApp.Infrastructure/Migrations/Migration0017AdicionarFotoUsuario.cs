using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Adiciona a foto de perfil do usuário: caminho relativo à pasta pública de
/// imagens da API (ex.: "imagens/abc123.jpg"). Nulo = sem foto.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(17, "Adiciona a foto de perfil do usuário")]
public class Migration0017AdicionarFotoUsuario : Migration
{
    public override void Up()
    {
        Alter.Table("usuarios")
            .AddColumn("caminho_foto").AsString(300).Nullable();
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
