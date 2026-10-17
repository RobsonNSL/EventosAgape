namespace AgapeApp.Application.EventoProdutos.DTOs;

/// <summary>
/// Produto vinculado a um evento, com o preço praticado e o estoque atual.
/// </summary>
public record EventoProdutoDTO(
    Guid Id,
    Guid EventoId,
    Guid ProdutoId,
    string ProdutoNome,
    decimal Preco,
    bool ControlaEstoque,
    int QuantidadeEstoque);
