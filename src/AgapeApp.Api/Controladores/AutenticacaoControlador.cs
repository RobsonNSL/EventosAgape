using System.Security.Claims;
using AgapeApp.Aplicacao.Autenticacao.Comandos.Login;
using AgapeApp.Aplicacao.Autenticacao.Comandos.RenovarToken;
using AgapeApp.Aplicacao.Autenticacao.Comandos.Sair;
using AgapeApp.Aplicacao.Autenticacao.Consultas.UsuarioAtual;
using AgapeApp.Aplicacao.Autenticacao.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controladores;

/// <summary>
/// Endpoints de autenticação: login, renovação de sessão, saída e dados do usuário atual.
/// </summary>
[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoControlador : ControllerBase
{
    private readonly ISender _mediador;

    public AutenticacaoControlador(ISender mediador)
    {
        _mediador = mediador;
    }

    /// <summary>Autentica o usuário com e-mail e senha e retorna os tokens de sessão.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespostaLogin), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaLogin>> Login(
        [FromBody] ComandoLogin comando,
        CancellationToken cancellationToken)
        => Ok(await _mediador.Send(comando, cancellationToken));

    /// <summary>Renova a sessão a partir do token de renovação (com rotação do token).</summary>
    [HttpPost("renovar")]
    [ProducesResponseType(typeof(RespostaLogin), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaLogin>> Renovar(
        [FromBody] ComandoRenovarToken comando,
        CancellationToken cancellationToken)
        => Ok(await _mediador.Send(comando, cancellationToken));

    /// <summary>Encerra a sessão revogando o token de renovação.</summary>
    [HttpPost("sair")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sair(
        [FromBody] ComandoSair comando,
        CancellationToken cancellationToken)
    {
        await _mediador.Send(comando, cancellationToken);
        return NoContent();
    }

    /// <summary>Retorna os dados do usuário autenticado.</summary>
    [HttpGet("eu")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioAutenticado), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioAutenticado>> Eu(CancellationToken cancellationToken)
    {
        var identificador = User.FindFirstValue("sub");

        if (!Guid.TryParse(identificador, out var usuarioId))
            return Unauthorized();

        return Ok(await _mediador.Send(new ConsultaUsuarioAtual(usuarioId), cancellationToken));
    }
}
