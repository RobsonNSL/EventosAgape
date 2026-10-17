using AgapeApp.Application.Usuarios.DTOs;
using MediatR;

namespace AgapeApp.Application.Usuarios.Queries.ListarUsuarios;

/// <summary>
/// Lista todos os usuários cadastrados. Somente administradores.
/// </summary>
public record ListarUsuariosQuery : IRequest<IReadOnlyList<UsuarioDTO>>;
