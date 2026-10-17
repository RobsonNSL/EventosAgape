using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos saldos de cartão por evento.
/// </summary>
public interface ISaldoCartaoEventoRepository
{
    void Adicionar(SaldoCartaoEvento saldo);

    /// <summary>
    /// Obtém o saldo para alteração (entidade rastreada: crédito na recarga, débito na venda).
    /// </summary>
    Task<SaldoCartaoEvento?> ObterAsync(
        Guid cartaoId,
        Guid eventoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista os saldos em circulação do evento (somente os maiores que zero), para o relatório.
    /// </summary>
    Task<IReadOnlyList<SaldoCartaoEvento>> ListarPorEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default);
}
