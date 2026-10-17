using AgapeApp.Api.Extensions;
using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Vendas.Commands.RegistrarVenda;
using AgapeApp.Application.Vendas.DTOs;
using AgapeApp.Application.Vendas.Queries.ListarVendasDoEvento;
using AgapeApp.Application.Vendas.Queries.ObterVenda;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Vendas de produtos. O operador vende no evento aberto em que está designado.
/// </summary>
[ApiController]
[Route("api")]
public class VendasController : ControllerBase
{
    private readonly ISender _mediator;

    public VendasController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registra uma venda (operador autenticado). Idempotente por IdOperacaoCliente:
    /// reenviar a mesma operação devolve a venda já registrada, sem debitar o saldo de novo.
    /// </summary>
    [HttpPost("vendas")]
    [Authorize]
    [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<VendaResponse>> Registrar(
        [FromBody] RegistrarVendaCommand comando,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        comando = comando with { UsuarioId = usuarioId.Value };

        return Ok(await _mediator.Send(comando, cancellationToken));
    }

    /// <summary>Lista as vendas de um evento (mais recentes primeiro), de forma paginada.</summary>
    [HttpGet("eventos/{eventoId:guid}/vendas")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(ResultadoPaginadoDTO<VendaDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginadoDTO<VendaDTO>>> Listar(
        Guid eventoId,
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var consulta = new ListarVendasDoEventoQuery(
            eventoId,
            pagina <= 0 ? 1 : pagina,
            tamanhoPagina <= 0 ? 50 : tamanhoPagina);

        return Ok(await _mediator.Send(consulta, cancellationToken));
    }

    /// <summary>Obtém o detalhe de uma venda com os itens (repasse e conferência).</summary>
    [HttpGet("vendas/{id:guid}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(VendaDetalheDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<VendaDetalheDTO>> Obter(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ObterVendaQuery(id), cancellationToken));
}
