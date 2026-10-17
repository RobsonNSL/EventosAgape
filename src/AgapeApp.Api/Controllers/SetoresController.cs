using AgapeApp.Application.EventoProdutos.DTOs;
using AgapeApp.Application.Setores.Commands.CriarSetor;
using AgapeApp.Application.Setores.Commands.RenomearSetor;
using AgapeApp.Application.Setores.DTOs;
using AgapeApp.Application.Setores.Queries.ListarSetores;
using AgapeApp.Application.SetorProdutos.Commands.DesvincularProdutoDoSetor;
using AgapeApp.Application.SetorProdutos.Commands.VincularProdutoAoSetor;
using AgapeApp.Application.SetorProdutos.DTOs;
using AgapeApp.Application.SetorProdutos.Queries.ListarProdutosDoSetor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Setores dos eventos e os produtos que cada setor vende.
/// O catálogo de produtos do setor é usado pelos operadores na tela de venda.
/// </summary>
[ApiController]
[Route("api")]
public class SetoresController : ControllerBase
{
    private readonly ISender _mediator;

    public SetoresController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Cria um setor em um evento (não permitido em evento encerrado).</summary>
    [HttpPost("eventos/{eventoId:guid}/setores")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(SetorDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<SetorDTO>> Criar(
        Guid eventoId,
        [FromBody] CriarSetorCommand comando,
        CancellationToken cancellationToken)
    {
        comando = comando with { EventoId = eventoId };

        var setor = await _mediator.Send(comando, cancellationToken);
        return Created($"/api/eventos/{eventoId}/setores", setor);
    }

    /// <summary>Lista os setores de um evento.</summary>
    [HttpGet("eventos/{eventoId:guid}/setores")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(IReadOnlyList<SetorDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SetorDTO>>> Listar(
        Guid eventoId,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarSetoresQuery(eventoId), cancellationToken));

    /// <summary>Renomeia um setor (não permitido em evento encerrado).</summary>
    [HttpPut("setores/{id:guid}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(SetorDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<SetorDTO>> Renomear(
        Guid id,
        [FromBody] RenomearSetorCommand comando,
        CancellationToken cancellationToken)
    {
        comando = comando with { Id = id };
        return Ok(await _mediator.Send(comando, cancellationToken));
    }

    /// <summary>Lista os produtos que o setor vende (usado pelos operadores na venda).</summary>
    [HttpGet("setores/{id:guid}/produtos")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<EventoProdutoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventoProdutoDTO>>> ListarProdutos(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarProdutosDoSetorQuery(id), cancellationToken));

    /// <summary>Vincula um produto do evento ao setor (o produto passa a ser vendido no setor).</summary>
    [HttpPost("setores/{id:guid}/produtos")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(SetorProdutoDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<SetorProdutoDTO>> VincularProduto(
        Guid id,
        [FromBody] VincularProdutoAoSetorCommand comando,
        CancellationToken cancellationToken)
    {
        comando = comando with { SetorId = id };

        var vinculo = await _mediator.Send(comando, cancellationToken);
        return Created($"/api/setores/{id}/produtos", vinculo);
    }

    /// <summary>Remove o vínculo de um produto com o setor.</summary>
    [HttpDelete("setores/{id:guid}/produtos/{eventoProdutoId:guid}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DesvincularProduto(
        Guid id,
        Guid eventoProdutoId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DesvincularProdutoDoSetorCommand(id, eventoProdutoId), cancellationToken);
        return NoContent();
    }
}
