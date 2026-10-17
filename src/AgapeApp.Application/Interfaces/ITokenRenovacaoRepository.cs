using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de tokens de renovação.
/// </summary>
public interface ITokenRenovacaoRepository
{
    void Adicionar(TokenRenovacao token);

    Task<TokenRenovacao?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
