namespace AgapeApp.Application.Recargas.DTOs;

/// <summary>
/// Resposta da recarga: identificador da recarga, cartão e saldo atual no evento.
/// Em retry idempotente, devolve a mesma recarga já registrada (mesmo RecargaId).
/// </summary>
public record RecargaResponse(
    Guid RecargaId,
    string CartaoCodigo,
    Guid EventoId,
    decimal Valor,
    decimal SaldoAtual,
    DateTimeOffset CriadoEm);
