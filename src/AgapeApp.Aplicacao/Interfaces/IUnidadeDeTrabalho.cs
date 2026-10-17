namespace AgapeApp.Aplicacao.Interfaces;

/// <summary>
/// Unidade de trabalho: confirma as alterações pendentes no banco de dados.
/// </summary>
public interface IUnidadeDeTrabalho
{
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
