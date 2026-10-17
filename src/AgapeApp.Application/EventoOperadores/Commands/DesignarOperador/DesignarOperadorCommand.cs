using AgapeApp.Application.EventoOperadores.DTOs;
using MediatR;

namespace AgapeApp.Application.EventoOperadores.Commands.DesignarOperador;

/// <summary>
/// Designa um operador a um setor do evento. Somente administradores.
/// O campo SolicitanteId é preenchido pela API com o usuário autenticado.
/// </summary>
public record DesignarOperadorCommand(
    Guid EventoId,
    Guid UsuarioId,
    Guid SetorId,
    Guid SolicitanteId) : IRequest<EventoOperadorDTO>;
