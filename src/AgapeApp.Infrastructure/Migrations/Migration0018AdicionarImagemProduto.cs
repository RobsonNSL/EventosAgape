using FluentMigrator;

namespace AgapeApp.Infrastructure.Migrations;

/// <summary>
/// Adiciona a imagem do produto: caminho relativo à pasta pública de imagens da API
/// (ex.: "imagens/produtos/coca-cola-lata.jpg"). Nulo = sem imagem.
/// Migrations são somente Up (forward-only): o rollback é feito manualmente quando necessário.
/// </summary>
[Migration(18, "Adiciona a imagem do produto")]
public class Migration0018AdicionarImagemProduto : Migration
{
    public override void Up()
    {
        Alter.Table("produtos")
            .AddColumn("caminho_imagem").AsString(300).Nullable();
    }

    // Forward-only por decisão do projeto: o Down não é implementado de propósito.
    // Para reverter, faça manualmente no banco (ou crie uma nova migration de correção).
    public override void Down()
        => throw new NotSupportedException(
            "As migrations do AgapeApp são somente Up (forward-only). O rollback é feito manualmente.");
}
