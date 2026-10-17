using AgapeApp.Application.Usuarios.DTOs;
using AgapeApp.Domain.Enums;
using MediatR;

namespace AgapeApp.Application.Usuarios.Commands.CriarUsuario;

/// <summary>
/// Cria um novo usuário (administrador ou operador). Somente administradores.
/// </summary>
public record CriarUsuarioCommand(
    string Nome,
    string Email,
    string Senha,
    PerfilUsuario Perfil) : IRequest<UsuarioDTO>;
