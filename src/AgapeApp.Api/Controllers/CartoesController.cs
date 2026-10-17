using AgapeApp.Application.Cartoes.Commands.AlterarAtivacaoCartao;
using AgapeApp.Application.Cartoes.Commands.CriarCartao;
using AgapeApp.Application.Cartoes.DTOs;
using AgapeApp.Application.Cartoes.Queries.ListarCartoes;
using AgapeApp.Application.Cartoes.Queries.ObterCartao;
using AgapeApp.Application.Cartoes.Queries.ObterSaldoCartao;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Cartões: cadastro (somente administradores) e consulta de saldo (operadores).
/// </summary>
[ApiController]
[Route("api/cartoes")]
public class CartoesController : ControllerBase
{
    private readonly ISender _mediator;

    public CartoesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Cadastra um novo cartão (um a um). O código é normalizado em maiúsculas.</summary>
    [HttpPost]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(CartaoDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<CartaoDTO>> Criar(
        [FromBody] CriarCartaoCommand comando,
        CancellationToken cancellationToken)
    {
        var cartao = await _mediator.Send(comando, cancellationToken);
        return Created($"/api/cartoes/{cartao.Id}", cartao);
    }

    /// <summary>Lista os cartões (filtro opcional por trecho do código; por padrão apenas os ativos).</summary>
    [HttpGet]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(IReadOnlyList<CartaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CartaoDTO>>> Listar(
        [FromQuery] string? codigo,
        [FromQuery] bool incluirInativos,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarCartoesQuery(codigo, incluirInativos), cancellationToken));

    /// <summary>Obtém um cartão pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(CartaoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartaoDTO>> Obter(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ObterCartaoQuery(id), cancellationToken));

    /// <summary>
    /// Consulta o saldo do cartão em um evento (o operador usa após ler o QR code;
    /// cartão ainda sem recarga no evento devolve saldo zero).
    /// </summary>
    [HttpGet("{codigo}/saldo")]
    [Authorize]
    [ProducesResponseType(typeof(CartaoSaldoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartaoSaldoDTO>> Saldo(
        string codigo,
        [FromQuery] Guid eventoId,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ObterSaldoCartaoQuery(codigo, eventoId), cancellationToken));

    /// <summary>Ativa um cartão.</summary>
    [HttpPost("{id:guid}/ativar")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(CartaoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartaoDTO>> Ativar(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AlterarAtivacaoCartaoCommand(id, true), cancellationToken));

    /// <summary>Desativa um cartão (exclusão lógica).</summary>
    [HttpPost("{id:guid}/desativar")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(CartaoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartaoDTO>> Desativar(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AlterarAtivacaoCartaoCommand(id, false), cancellationToken));
}
