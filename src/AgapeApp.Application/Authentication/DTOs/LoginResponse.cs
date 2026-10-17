namespace AgapeApp.Application.Authentication.DTOs;

/// <summary>
/// Resposta de autenticação: tokens de acesso/renovação e dados do usuário.
/// </summary>
public record LoginResponse(
    string TokenAcesso,
    string TokenRenovacao,
    DateTimeOffset ExpiraEm,
    UsuarioAutenticadoDTO Usuario);
