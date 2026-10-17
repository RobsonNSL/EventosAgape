namespace AgapeApp.Application.Vendas.DTOs;

/// <summary>
/// Dados de uma venda para listagem/relatórios.
/// </summary>
public record VendaDTO(
    Guid Id,
    string CartaoCodigo,
    string SetorNome,
    string UsuarioNome,
    decimal Total,
    int QuantidadeItens,
    DateTimeOffset CriadoEm);
