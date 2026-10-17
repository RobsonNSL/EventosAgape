namespace AgapeApp.Aplicacao.Autenticacao.Dtos;

/// <summary>
/// Resposta de autenticação: tokens de acesso/renovação e dados do usuário.
/// </summary>
public record RespostaLogin(
    string TokenAcesso,
    string TokenRenovacao,
    DateTimeOffset ExpiraEm,
    UsuarioAutenticado Usuario);
