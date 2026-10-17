using MediatR;

namespace AgapeApp.Application.Authentication.Commands.Sair;

/// <summary>
/// Encerra a sessão revogando o token de renovação informado.
/// </summary>
public record SairCommand(string TokenRenovacao) : IRequest<Unit>;
