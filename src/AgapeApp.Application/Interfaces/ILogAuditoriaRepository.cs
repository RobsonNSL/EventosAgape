using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos registros de auditoria.
/// </summary>
public interface ILogAuditoriaRepository
{
    void Adicionar(LogAuditoria log);

    Task<IReadOnlyList<LogAuditoria>> ListarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LogAuditoria>> ListarPorEventoAsync(
        Guid eventoId,
        int pular,
        int quantidade,
        CancellationToken cancellationToken = default);

    Task<int> ContarPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
}
