using MediatR;

namespace AgapeApp.Application.Usuarios.Commands.RedefinirSenhaUsuario;

/// <summary>
/// Redefine a senha de um usuário (funciona como "esqueci a senha" assistido).
/// Somente administradores. As sessões de renovação abertas do usuário são revogadas.
/// </summary>
public record RedefinirSenhaUsuarioCommand(
    Guid Id,
    string NovaSenha) : IRequest<Unit>;
