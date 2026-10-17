using AgapeApp.Dominio.Entidades;

namespace AgapeApp.Aplicacao.Interfaces;

/// <summary>
/// Acesso aos dados de tokens de renovação.
/// </summary>
public interface IRepositorioTokensRenovacao
{
    void Adicionar(TokenRenovacao token);

    Task<TokenRenovacao?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
