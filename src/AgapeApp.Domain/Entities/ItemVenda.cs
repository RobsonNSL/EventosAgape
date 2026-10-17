using AgapeApp.Domain.Common;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Item de uma venda. Guarda um retrato (snapshot) do nome e do preço no momento da venda:
/// mudar o preço do produto no meio do evento não altera vendas já registradas.
/// </summary>
public class ItemVenda : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private ItemVenda()
    {
    }

    public Guid VendaId { get; private set; }

    public Guid EventoProdutoId { get; private set; }

    /// <summary>Nome do produto no momento da venda.</summary>
    public string ProdutoNome { get; private set; } = null!;

    /// <summary>Preço unitário praticado no momento da venda.</summary>
    public decimal PrecoUnitario { get; private set; }

    public int Quantidade { get; private set; }

    public decimal Total { get; private set; }

    public static ItemVenda Criar(
        Guid vendaId,
        Guid eventoProdutoId,
        string produtoNome,
        decimal precoUnitario,
        int quantidade)
    {
        if (string.IsNullOrWhiteSpace(produtoNome))
            throw new RegraNegocioException(MensagensErro.NomeObrigatorio);

        if (precoUnitario <= 0)
            throw new RegraNegocioException(MensagensErro.PrecoInvalido);

        if (quantidade <= 0)
            throw new RegraNegocioException(MensagensErro.QuantidadeInvalida);

        return new ItemVenda
        {
            VendaId = vendaId,
            EventoProdutoId = eventoProdutoId,
            ProdutoNome = produtoNome.Trim(),
            PrecoUnitario = precoUnitario,
            Quantidade = quantidade,
            Total = precoUnitario * quantidade
        };
    }
}
