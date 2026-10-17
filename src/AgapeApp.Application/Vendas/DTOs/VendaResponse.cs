namespace AgapeApp.Application.Vendas.DTOs;

/// <summary>
/// Resposta da venda: identificador, itens, total e saldo atual do cartão no evento.
/// Em retry idempotente, devolve a mesma venda já registrada (mesmo VendaId).
/// </summary>
public record VendaResponse(
    Guid VendaId,
    string CartaoCodigo,
    Guid EventoId,
    decimal Total,
    decimal SaldoAtual,
    IReadOnlyList<ItemVendaDTO> Itens,
    DateTimeOffset CriadoEm);
