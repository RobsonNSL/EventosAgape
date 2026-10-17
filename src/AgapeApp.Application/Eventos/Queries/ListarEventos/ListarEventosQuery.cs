using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Domain.Enums;
using MediatR;

namespace AgapeApp.Application.Eventos.Queries.ListarEventos;

/// <summary>
/// Lista os eventos, com filtro opcional por situação. Somente administradores.
/// </summary>
public record ListarEventosQuery(SituacaoEvento? Situacao = null) : IRequest<IReadOnlyList<EventoDTO>>;
