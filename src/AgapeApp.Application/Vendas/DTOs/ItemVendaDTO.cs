namespace AgapeApp.Application.Vendas.DTOs;

/// <summary>
/// Item de venda com os dados registrados no momento da venda (retrato do preço).
/// </summary>
public record ItemVendaDTO(
    Guid EventoProdutoId,
    string ProdutoNome,
    decimal PrecoUnitario,
    int Quantidade,
    decimal Total);
