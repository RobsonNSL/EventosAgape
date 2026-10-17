namespace AgapeApp.Application.Usuarios.DTOs;

/// <summary>
/// Dados de um usuário para listagem/administração.
/// </summary>
public record UsuarioDTO(
    Guid Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    DateTimeOffset CriadoEm);
