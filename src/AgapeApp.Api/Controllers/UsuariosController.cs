using AgapeApp.Api.Extensions;
using AgapeApp.Application.Usuarios.Commands.AlterarAtivacaoUsuario;
using AgapeApp.Application.Usuarios.Commands.AtualizarUsuario;
using AgapeApp.Application.Usuarios.Commands.CriarUsuario;
using AgapeApp.Application.Usuarios.Commands.RedefinirSenhaUsuario;
using AgapeApp.Application.Usuarios.Queries.ListarUsuarios;
using AgapeApp.Application.Usuarios.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Administração de usuários. Acesso exclusivo de administradores.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = "SomenteAdministrador")]
public class UsuariosController : ControllerBase
{
    private readonly ISender _mediator;

    public UsuariosController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Cria um novo usuário (administrador ou operador).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<UsuarioDTO>> Criar(
        [FromBody] CriarUsuarioCommand comando,
        CancellationToken cancellationToken)
    {
        var usuario = await _mediator.Send(comando, cancellationToken);
        return Created($"/api/usuarios/{usuario.Id}", usuario);
    }

    /// <summary>Lista todos os usuários cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDTO>>> Listar(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarUsuariosQuery(), cancellationToken));

    /// <summary>Atualiza os dados cadastrais de um usuário (nome, e-mail e perfil).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioDTO>> Atualizar(
        Guid id,
        [FromBody] AtualizarUsuarioCommand comando,
        CancellationToken cancellationToken)
    {
        comando = comando with { Id = id };
        return Ok(await _mediator.Send(comando, cancellationToken));
    }

    /// <summary>Ativa um usuário.</summary>
    [HttpPost("{id:guid}/ativar")]
    [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioDTO>> Ativar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        return Ok(await _mediator.Send(
            new AlterarAtivacaoUsuarioCommand(id, true, usuarioId.Value),
            cancellationToken));
    }

    /// <summary>Desativa um usuário (exclusão lógica). O próprio solicitante não pode ser desativado.</summary>
    [HttpPost("{id:guid}/desativar")]
    [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioDTO>> Desativar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        return Ok(await _mediator.Send(
            new AlterarAtivacaoUsuarioCommand(id, false, usuarioId.Value),
            cancellationToken));
    }

    /// <summary>Redefine a senha de um usuário (as sessões abertas dele são encerradas).</summary>
    [HttpPost("{id:guid}/redefinir-senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RedefinirSenha(
        Guid id,
        [FromBody] RedefinirSenhaUsuarioCommand comando,
        CancellationToken cancellationToken)
    {
        comando = comando with { Id = id };
        await _mediator.Send(comando, cancellationToken);
        return NoContent();
    }
}
