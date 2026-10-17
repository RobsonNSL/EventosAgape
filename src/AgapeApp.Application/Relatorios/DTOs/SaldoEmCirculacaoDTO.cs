namespace AgapeApp.Application.Relatorios.DTOs;

/// <summary>
/// Saldo ainda disponível em um cartão no evento (saldo em circulação).
/// </summary>
public record SaldoEmCirculacaoDTO(
    Guid CartaoId,
    string CartaoCodigo,
    decimal Saldo);
