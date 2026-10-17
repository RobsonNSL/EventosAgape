using AgapeApp.Application.Usuarios.DTOs;
using MediatR;

namespace AgapeApp.Application.Usuarios.Commands.AlterarAtivacaoUsuario;

/// <summary>
/// Ativa ou desativa um usuário (exclusão lógica: nada é apagado de verdade).
/// Somente administradores. O próprio usuário não pode ser desativado; o
/// sistema também não pode ficar sem administrador ativo.
/// </summary>
public record AlterarAtivacaoUsuarioCommand(
    Guid Id,
    bool Ativo,
    Guid SolicitanteId) : IRequest<UsuarioDTO>;
