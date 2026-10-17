namespace AgapeApp.Application.Produtos.DTOs;

/// <summary>
/// Dados de um produto para listagem/administração.
/// </summary>
public record ProdutoDTO(
    Guid Id,
    string Nome,
    decimal? PrecoPadrao,
    bool Ativo,
    DateTimeOffset CriadoEm);
