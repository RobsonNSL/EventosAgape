using AgapeApp.Application.Eventos.DTOs;
using MediatR;

namespace AgapeApp.Application.Eventos.Commands.EncerrarEvento;

/// <summary>
/// Encerra o evento (somente a partir de Aberto). Somente administradores.
/// O campo UsuarioId é preenchido pela API com o usuário autenticado.
/// </summary>
public record EncerrarEventoCommand(
    Guid Id,
    Guid UsuarioId) : IRequest<EventoDTO>;
