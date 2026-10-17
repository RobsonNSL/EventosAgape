using AgapeApp.Application.Authentication.DTOs;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.Login;

/// <summary>
/// Autentica um usuário com e-mail e senha.
/// </summary>
public record LoginCommand(
    string Email,
    string Senha,
    string? Dispositivo = null) : IRequest<LoginResponse>;
