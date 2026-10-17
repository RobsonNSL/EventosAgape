using AgapeApp.Api.Extensions;
using AgapeApp.Application.EventoOperadores.Commands.AtualizarDesignacaoOperador;
using AgapeApp.Application.EventoOperadores.Commands.DesignarOperador;
using AgapeApp.Application.EventoOperadores.DTOs;
using AgapeApp.Application.EventoOperadores.Queries.ListarOperadoresDoEvento;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Operadores designados aos eventos (setor e habilitação). Acesso exclusivo de administradores.
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Policy = "SomenteAdministrador")]
public class EventoOperadoresController : ControllerBase
{
    private readonly ISender _mediator;

    public EventoOperadoresController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Designa um operador a um setor do evento.</summary>
    [HttpPost("eventos/{eventoId:guid}/operadores")]
    [ProducesResponseType(typeof(EventoOperadorDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<EventoOperadorDTO>> Designar(
        Guid eventoId,
        [FromBody] DesignarOperadorCommand comando,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        comando = comando with { EventoId = eventoId, SolicitanteId = usuarioId.Value };

        var designacao = await _mediator.Send(comando, cancellationToken);
        return Created($"/api/eventos/{eventoId}/operadores", designacao);
    }

    /// <summary>Lista os operadores designados ao evento.</summary>
    [HttpGet("eventos/{eventoId:guid}/operadores")]
    [ProducesResponseType(typeof(IReadOnlyList<EventoOperadorDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventoOperadorDTO>>> Listar(
        Guid eventoId,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarOperadoresDoEventoQuery(eventoId), cancellationToken));

    /// <summary>Troca o setor e/ou habilita/desabilita o operador no evento.</summary>
    [HttpPut("evento-operadores/{id:guid}")]
    [ProducesResponseType(typeof(EventoOperadorDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventoOperadorDTO>> Atualizar(
        Guid id,
        [FromBody] AtualizarDesignacaoOperadorCommand comando,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        comando = comando with { Id = id, SolicitanteId = usuarioId.Value };
        return Ok(await _mediator.Send(comando, cancellationToken));
    }
}
