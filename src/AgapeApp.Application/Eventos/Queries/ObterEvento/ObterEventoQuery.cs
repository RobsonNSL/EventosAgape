using AgapeApp.Application.Eventos.DTOs;
using MediatR;

namespace AgapeApp.Application.Eventos.Queries.ObterEvento;

/// <summary>
/// Obtém um evento pelo identificador. Somente administradores.
/// </summary>
public record ObterEventoQuery(Guid Id) : IRequest<EventoDTO>;
