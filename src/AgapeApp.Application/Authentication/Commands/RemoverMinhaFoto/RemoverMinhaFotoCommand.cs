using AgapeApp.Application.Usuarios.DTOs;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.RemoverMinhaFoto;

/// <summary>
/// Remove a foto de perfil do próprio usuário autenticado (idempotente).
/// </summary>
public record RemoverMinhaFotoCommand(Guid UsuarioId) : IRequest<UsuarioDTO>;
