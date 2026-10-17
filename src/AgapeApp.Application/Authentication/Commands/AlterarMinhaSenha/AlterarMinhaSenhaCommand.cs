using MediatR;

namespace AgapeApp.Application.Authentication.Commands.AlterarMinhaSenha;

/// <summary>
/// Troca a senha do próprio usuário autenticado (exige a senha atual).
/// As sessões de renovação abertas são revogadas; o token de acesso atual
/// continua valendo até expirar (30 minutos).
/// </summary>
public record AlterarMinhaSenhaCommand(
    Guid UsuarioId,
    string SenhaAtual,
    string NovaSenha) : IRequest<Unit>;
