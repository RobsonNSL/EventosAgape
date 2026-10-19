using AgapeApp.Domain.Common;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Produto vendável nos eventos (ex.: pastel, refrigerante).
/// O preço praticado é definido no vínculo com o evento (<see cref="EventoProduto"/>).
/// </summary>
public class Produto : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private Produto()
    {
    }

    public string Nome { get; private set; } = null!;

    /// <summary>Preço sugerido para pré-preencher o vínculo com o evento (opcional).</summary>
    public decimal? PrecoPadrao { get; private set; }

    public bool Ativo { get; private set; }

    /// <summary>
    /// Imagem do produto: caminho relativo à pasta pública da API
    /// (ex.: "imagens/produtos/coca-cola-lata.jpg"). Nulo = sem imagem.
    /// </summary>
    public string? CaminhoImagem { get; private set; }

    /// <summary>
    /// Cria um novo produto ativo.
    /// </summary>
    public static Produto Criar(string nome, decimal? precoPadrao)
    {
        ValidarNome(nome);
        ValidarPrecoPadrao(precoPadrao);

        return new Produto
        {
            Nome = nome.Trim(),
            PrecoPadrao = precoPadrao,
            Ativo = true
        };
    }

    /// <summary>
    /// Atualiza nome e preço padrão do produto.
    /// </summary>
    public void Atualizar(string nome, decimal? precoPadrao)
    {
        ValidarNome(nome);
        ValidarPrecoPadrao(precoPadrao);

        Nome = nome.Trim();
        PrecoPadrao = precoPadrao;
        MarcarComoAtualizada();
    }

    /// <summary>Define (ou limpa, com nulo/vazio) a imagem do produto.</summary>
    public void DefinirImagem(string? caminhoImagem)
    {
        CaminhoImagem = string.IsNullOrWhiteSpace(caminhoImagem) ? null : caminhoImagem.Trim();
        MarcarComoAtualizada();
    }

    public void Ativar()
    {
        Ativo = true;
        MarcarComoAtualizada();
    }

    public void Desativar()
    {
        Ativo = false;
        MarcarComoAtualizada();
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException(MensagensErro.NomeObrigatorio);
    }

    private static void ValidarPrecoPadrao(decimal? precoPadrao)
    {
        if (precoPadrao is <= 0)
            throw new RegraNegocioException(MensagensErro.PrecoInvalido);
    }
}
