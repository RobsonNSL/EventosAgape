namespace AgapeApp.Application.Relatorios.DTOs;

/// <summary>
/// Consolidado de vendas de um produto no evento.
/// </summary>
public record ResumoPorProdutoDTO(
    Guid EventoProdutoId,
    string ProdutoNome,
    int QuantidadeVendida,
    decimal TotalVendido);
