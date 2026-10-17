using AgapeApp.Dominio.Comum;
using AgapeApp.Dominio.Excecoes;

namespace AgapeApp.Dominio.Entidades;

/// <summary>
/// Token de renovação de sessão (refresh token) vinculado a um usuário.
/// O valor bruto do token nunca é armazenado: apenas o hash.
/// </summary>
public class TokenRenovacao : EntidadeBase
{
    // Construtor privado exigido pelo Entity Framework.
    private TokenRenovacao()
    {
    }

    public Guid UsuarioId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiraEm { get; private set; }

    public DateTimeOffset? RevogadoEm { get; private set; }

    public string? Dispositivo { get; private set; }

    /// <summary>
    /// Cria um novo token de renovação.
    /// </summary>
    public static TokenRenovacao Criar(Guid usuarioId, string tokenHash, DateTimeOffset expiraEm, string? dispositivo = null)
    {
        if (usuarioId == Guid.Empty)
            throw new ExcecaoRegraNegocio("O usuário do token de renovação é obrigatório.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ExcecaoRegraNegocio("O hash do token de renovação é obrigatório.");

        return new TokenRenovacao
        {
            UsuarioId = usuarioId,
            TokenHash = tokenHash,
            ExpiraEm = expiraEm,
            Dispositivo = dispositivo
        };
    }

    /// <summary>
    /// Verifica se o token ainda pode ser utilizado (não revogado e não expirado).
    /// </summary>
    public bool EstaValido(DateTimeOffset agora) => RevogadoEm is null && ExpiraEm > agora;

    /// <summary>
    /// Revoga o token. Operação idempotente: revogar duas vezes não tem efeito adicional.
    /// </summary>
    public void Revogar()
    {
        if (RevogadoEm is not null)
            return;

        RevogadoEm = DateTimeOffset.UtcNow;
        MarcarComoAtualizada();
    }
}
