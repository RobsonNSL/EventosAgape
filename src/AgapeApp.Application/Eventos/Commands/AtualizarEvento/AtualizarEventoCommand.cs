using AgapeApp.Application.Eventos.DTOs;
using MediatR;

namespace AgapeApp.Application.Eventos.Commands.AtualizarEvento;

/// <summary>
/// Atualiza nome e datas de um evento (não permitido em evento encerrado). Somente administradores.
/// </summary>
public record AtualizarEventoCommand(
    Guid Id,
    string Nome,
    DateOnly DataInicio,
    DateOnly? DataFim) : IRequest<EventoDTO>;
