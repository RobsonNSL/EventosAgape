using System.Security.Claims;
using AgapeApp.Application.Authentication.Commands.Login;
using AgapeApp.Application.Authentication.Commands.RenovarToken;
using AgapeApp.Application.Authentication.Commands.Sair;
using AgapeApp.Application.Authentication.Queries.UsuarioAtual;
using AgapeApp.Application.Authentication.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Endpoints de autenticação: login, renovação de sessão, saída e dados do usuário atual.
/// </summary>
[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly ISender _mediator;

    public AutenticacaoController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Autentica o usuário com e-mail e senha e retorna os tokens de sessão.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginCommand comando,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(comando, cancellationToken));

    /// <summary>Renova a sessão a partir do token de renovação (com rotação do token).</summary>
    [HttpPost("renovar")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Renovar(
        [FromBody] RenovarTokenCommand comando,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(comando, cancellationToken));

    /// <summary>Encerra a sessão revogando o token de renovação.</summary>
    [HttpPost("sair")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sair(
        [FromBody] SairCommand comando,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(comando, cancellationToken);
        return NoContent();
    }

    /// <summary>Retorna os dados do usuário autenticado.</summary>
    [HttpGet("eu")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioAutenticadoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioAutenticadoDTO>> Eu(CancellationToken cancellationToken)
    {
        var identificador = User.FindFirstValue("sub");

        if (!Guid.TryParse(identificador, out var usuarioId))
            return Unauthorized();

        return Ok(await _mediator.Send(new ObterUsuarioAtualQuery(usuarioId), cancellationToken));
    }
}
