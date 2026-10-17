using AgapeApp.Mobile.Helpers;

namespace AgapeApp.Mobile.Models;

/// <summary>Linha da lista de itens na confirmação da venda.</summary>
public class ItemVendaCartao
{
    public required string Nome { get; init; }
    public required int Quantidade { get; init; }
    public required decimal Subtotal { get; init; }

    public string TextoQuantidade => $"{Quantidade}x";

    public string TextoSubtotal => Formatos.Moeda(Subtotal);
}
