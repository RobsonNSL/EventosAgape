using AgapeApp.Api.Extensions;
using AgapeApp.Application.Eventos.Commands.AbrirEvento;
using AgapeApp.Application.Eventos.Commands.AtualizarEvento;
using AgapeApp.Application.Eventos.Commands.CriarEvento;
using AgapeApp.Application.Eventos.Commands.EncerrarEvento;
using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Application.Eventos.Queries.ListarEventos;
using AgapeApp.Application.Eventos.Queries.ListarMeusEventos;
using AgapeApp.Application.Eventos.Queries.ObterEvento;
using AgapeApp.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Gestão de eventos: cadastro e ciclo de vida (Rascunho → Aberto → Encerrado).
/// </summary>
[ApiController]
[Route("api/eventos")]
public class EventosController : ControllerBase
{
    private readonly ISender _mediator;

    public EventosController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Cria um novo evento em rascunho.</summary>
    [HttpPost]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(EventoDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<EventoDTO>> Criar(
        [FromBody] CriarEventoCommand comando,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        comando = comando with { CriadoPor = usuarioId.Value };

        var evento = await _mediator.Send(comando, cancellationToken);
        return Created($"/api/eventos/{evento.Id}", evento);
    }

    /// <summary>Lista os eventos, com filtro opcional por situação (ex.: Aberto).</summary>
    [HttpGet]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(IReadOnlyList<EventoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventoDTO>>> Listar(
        [FromQuery] SituacaoEvento? situacao,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarEventosQuery(situacao), cancellationToken));

    /// <summary>Lista os eventos em que o usuário autenticado está designado como operador.</summary>
    [HttpGet("meus")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<MeuEventoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MeuEventoDTO>>> Meus(CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        return Ok(await _mediator.Send(new ListarMeusEventosQuery(usuarioId.Value), cancellationToken));
    }

    /// <summary>Obtém um evento pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(EventoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventoDTO>> Obter(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ObterEventoQuery(id), cancellationToken));

    /// <summary>Atualiza nome e datas do evento (não permitido em evento encerrado; o id vem da rota).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(EventoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventoDTO>> Atualizar(
        Guid id,
        [FromBody] AtualizarEventoCommand comando,
        CancellationToken cancellationToken)
    {
        comando = comando with { Id = id };
        return Ok(await _mediator.Send(comando, cancellationToken));
    }

    /// <summary>Abre o evento para operações (somente a partir de rascunho).</summary>
    [HttpPost("{id:guid}/abrir")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(EventoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventoDTO>> Abrir(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        return Ok(await _mediator.Send(new AbrirEventoCommand(id, usuarioId.Value), cancellationToken));
    }

    /// <summary>Encerra o evento (somente a partir de aberto).</summary>
    [HttpPost("{id:guid}/encerrar")]
    [Authorize(Policy = "SomenteAdministrador")]
    [ProducesResponseType(typeof(EventoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<EventoDTO>> Encerrar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        if (usuarioId is null)
            return Unauthorized();

        return Ok(await _mediator.Send(new EncerrarEventoCommand(id, usuarioId.Value), cancellationToken));
    }
}
