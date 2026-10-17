namespace AgapeApp.Application.Authentication.DTOs;

/// <summary>
/// Dados básicos do usuário autenticado.
/// </summary>
public record UsuarioAutenticadoDTO(
    Guid Id,
    string Nome,
    string Email,
    string Perfil,
    string? CaminhoFoto);
