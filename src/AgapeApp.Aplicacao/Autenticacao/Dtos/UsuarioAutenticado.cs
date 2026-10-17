namespace AgapeApp.Aplicacao.Autenticacao.Dtos;

/// <summary>
/// Dados básicos do usuário autenticado.
/// </summary>
public record UsuarioAutenticado(
    Guid Id,
    string Nome,
    string Email,
    string Perfil);
