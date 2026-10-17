using AgapeApp.Api.Extensions;
using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Recargas.Commands.EfetuarRecarga;
using AgapeApp.Application.Recargas.DTOs;
using AgapeApp.Application.Recargas.Queries.ListarRecargas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Recargas de cartão. O operador recarrega no evento aberto em que está designado.
/// </summary>
[ApiController]
[Route("api")]
public class RecargasController : ControllerBase
{
    private readonly ISender _mediator;

    public RecargasController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registra uma recarga (operador autenticado). Idempotente por IdOperacaoCliente:
    /// reenviar a mesma operação devolve a recarga já registrada, sem duplicar o saldo.
    /// </summary>
    [HttpPost("recargas")]
    [Authorize]
    [ProducesResponseType(typeof(RecargaResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecargaResponse>> Efetuar(
        [FromBody] EfetuarRecargaCommand comando,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        comando = comando with { UsuarioId = usuarioId.Value };

        return Ok(await _mediator.Send(comando, cancellationToken));
    }

    /// <summary>Lista as recargas de um evento (mais recentes primeiro), de forma paginada.</summary>
    [HttpGet("eventos/{eventoId:guid}/recargas")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(ResultadoPaginadoDTO<RecargaDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginadoDTO<RecargaDTO>>> Listar(
        Guid eventoId,
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var consulta = new ListarRecargasQuery(
            eventoId,
            pagina <= 0 ? 1 : pagina,
            tamanhoPagina <= 0 ? 50 : tamanhoPagina);

        return Ok(await _mediator.Send(consulta, cancellationToken));
    }
}
