using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de tokens de renovação.
/// </summary>
public interface ITokenRenovacaoRepository
{
    void Adicionar(TokenRenovacao token);

    Task<TokenRenovacao?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoga todos os tokens de renovação ainda válidos do usuário
    /// (usado ao desativar o usuário ou ao redefinir a senha).
    /// </summary>
    Task RevogarTodosDoUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
