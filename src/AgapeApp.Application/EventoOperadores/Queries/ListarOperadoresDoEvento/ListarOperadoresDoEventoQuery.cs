using AgapeApp.Application.EventoOperadores.DTOs;
using MediatR;

namespace AgapeApp.Application.EventoOperadores.Queries.ListarOperadoresDoEvento;

/// <summary>
/// Lista os operadores designados ao evento, com setor e situação. Somente administradores.
/// </summary>
public record ListarOperadoresDoEventoQuery(Guid EventoId) : IRequest<IReadOnlyList<EventoOperadorDTO>>;
