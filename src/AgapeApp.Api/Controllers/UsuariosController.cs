using AgapeApp.Application.Usuarios.Commands.CriarUsuario;
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
}
