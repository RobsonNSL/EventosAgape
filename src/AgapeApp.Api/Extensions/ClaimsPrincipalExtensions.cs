using System.Security.Claims;

namespace AgapeApp.Api.Extensions;

/// <summary>
/// Leitura das claims do usuário autenticado (preenchidas pelo JWT: sub, nome, email, perfil).
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Identificador do usuário autenticado (claim "sub").</summary>
    public static Guid? ObterUsuarioId(this ClaimsPrincipal usuario)
        => Guid.TryParse(usuario.FindFirstValue("sub"), out var id) ? id : null;

    /// <summary>Perfil do usuário autenticado (claim "perfil").</summary>
    public static string? ObterPerfil(this ClaimsPrincipal usuario)
        => usuario.FindFirstValue("perfil");
}
