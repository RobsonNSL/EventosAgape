using AgapeApp.Application.Usuarios.DTOs;
using AgapeApp.Domain.Enums;
using MediatR;

namespace AgapeApp.Application.Usuarios.Commands.AtualizarUsuario;

/// <summary>
/// Atualiza os dados cadastrais de um usuário (nome, e-mail e perfil).
/// Somente administradores.
/// </summary>
public record AtualizarUsuarioCommand(
    Guid Id,
    string Nome,
    string Email,
    PerfilUsuario Perfil) : IRequest<UsuarioDTO>;
