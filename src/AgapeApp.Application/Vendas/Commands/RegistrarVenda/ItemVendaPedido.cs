namespace AgapeApp.Application.Vendas.Commands.RegistrarVenda;

/// <summary>
/// Item do pedido de venda. Itens repetidos do mesmo produto são somados em uma única linha.
/// </summary>
public record ItemVendaPedido(
    Guid EventoProdutoId,
    int Quantidade);
