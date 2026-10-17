using AgapeApp.Application.Authentication.DTOs;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.RenovarToken;

/// <summary>
/// Renova a sessão a partir de um token de renovação válido (com rotação do token).
/// </summary>
public record RenovarTokenCommand(
    string TokenRenovacao,
    string? Dispositivo = null) : IRequest<LoginResponse>;
