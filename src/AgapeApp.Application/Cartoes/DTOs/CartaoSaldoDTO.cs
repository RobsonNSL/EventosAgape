namespace AgapeApp.Application.Cartoes.DTOs;

/// <summary>
/// Saldo de um cartão em um evento — a leitura que o operador faz após ler o QR code.
/// </summary>
public record CartaoSaldoDTO(
    Guid CartaoId,
    string Codigo,
    bool Ativo,
    Guid EventoId,
    decimal SaldoAtual);
