using AgapeApp.Application.Auditoria.DTOs;
using AgapeApp.Application.Auditoria.Queries.ListarAuditoriaDoEvento;
using AgapeApp.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Auditoria do evento: quem fez o quê, quando e em qual dispositivo.
/// Acesso exclusivo de administradores.
/// </summary>
[ApiController]
[Route("api/eventos/{eventoId:guid}/auditoria")]
[Authorize(Policy = "SomenteAdministrador")]
public class AuditoriaController : ControllerBase
{
    private readonly ISender _mediator;

    public AuditoriaController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lista os registros de auditoria do evento (mais recentes primeiro), de forma paginada.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginadoDTO<LogAuditoriaDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginadoDTO<LogAuditoriaDTO>>> Listar(
        Guid eventoId,
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var consulta = new ListarAuditoriaDoEventoQuery(
            eventoId,
            pagina <= 0 ? 1 : pagina,
            tamanhoPagina <= 0 ? 50 : tamanhoPagina);

        return Ok(await _mediator.Send(consulta, cancellationToken));
    }
}
