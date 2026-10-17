using AgapeApp.Application.Eventos.DTOs;
using MediatR;

namespace AgapeApp.Application.Eventos.Commands.CriarEvento;

/// <summary>
/// Cria um novo evento em rascunho. Somente administradores.
/// O campo CriadoPor é preenchido pela API com o usuário autenticado.
/// </summary>
public record CriarEventoCommand(
    string Nome,
    DateOnly DataInicio,
    DateOnly? DataFim,
    Guid CriadoPor) : IRequest<EventoDTO>;
