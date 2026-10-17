namespace AgapeApp.Aplicacao.Usuarios.Dtos;

/// <summary>
/// Dados de um usuário para listagem/administração.
/// </summary>
public record UsuarioDetalhe(
    Guid Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    DateTimeOffset CriadoEm);
