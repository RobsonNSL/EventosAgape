namespace AgapeApp.Application.Vendas.DTOs;

/// <summary>
/// Detalhe de uma venda (repasse e conferência).
/// </summary>
public record VendaDetalheDTO(
    Guid Id,
    string CartaoCodigo,
    Guid EventoId,
    string SetorNome,
    string UsuarioNome,
    decimal Total,
    string? Dispositivo,
    IReadOnlyList<ItemVendaDTO> Itens,
    DateTimeOffset CriadoEm);
