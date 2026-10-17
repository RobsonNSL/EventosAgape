namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Unidade de trabalho: confirma as alterações pendentes no banco de dados.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
