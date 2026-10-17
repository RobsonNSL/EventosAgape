using AgapeApp.Api.Extensions;
using AgapeApp.Application.EventoProdutos.Commands.AjustarEstoqueEventoProduto;
using AgapeApp.Application.EventoProdutos.Commands.AtualizarEventoProduto;
using AgapeApp.Application.EventoProdutos.Commands.VincularProdutoAoEvento;
using AgapeApp.Application.EventoProdutos.DTOs;
using AgapeApp.Application.EventoProdutos.Queries.ListarProdutosDoEvento;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Produtos do evento: preço praticado, controle de estoque e ajustes manuais.
/// Acesso exclusivo de administradores.
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Policy = "SomenteAdministrador")]
public class EventoProdutosController : ControllerBase
{
    private readonly ISender _mediator;

    public EventoProdutosController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Vincula um produto ao evento com preço e, se controlar estoque, a quantidade inicial.</summary>
    [HttpPost("eventos/{eventoId:guid}/produtos")]
    [ProducesResponseType(typeof(EventoProdutoDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<EventoProdutoDTO>> Vincular(
        Guid eventoId,
        [FromBody] VincularProdutoAoEventoCommand comando,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        comando = comando with { EventoId = eventoId, UsuarioId = usuarioId.Value };

        var eventoProduto = await _mediator.Send(comando, cancellationToken);
        return Created($"/api/eventos/{eventoId}/produtos", eventoProduto);
    }

    /// <summary>Lista os produtos vinculados ao evento, com preço e estoque atual.</summary>
    [HttpGet("eventos/{eventoId:guid}/produtos")]
    [ProducesResponseType(typeof(IReadOnlyList<EventoProdutoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventoProdutoDTO>>> Listar(
        Guid eventoId,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarProdutosDoEventoQuery(eventoId), cancellationToken));

    /// <summary>Atualiza o preço praticado e o controle de estoque do produto no evento.</summary>
    [HttpPut("evento-produtos/{id:guid}")]
    [ProducesResponseType(typeof(EventoProdutoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventoProdutoDTO>> Atualizar(
        Guid id,
        [FromBody] AtualizarEventoProdutoCommand comando,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        comando = comando with { Id = id, UsuarioId = usuarioId.Value };
        return Ok(await _mediator.Send(comando, cancellationToken));
    }

    /// <summary>Ajuste manual de estoque (positivo para entrada, negativo para baixa).</summary>
    [HttpPost("evento-produtos/{id:guid}/ajuste-estoque")]
    [ProducesResponseType(typeof(EventoProdutoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventoProdutoDTO>> AjustarEstoque(
        Guid id,
        [FromBody] AjustarEstoqueEventoProdutoCommand comando,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        comando = comando with { Id = id, UsuarioId = usuarioId.Value };
        return Ok(await _mediator.Send(comando, cancellationToken));
    }
}
