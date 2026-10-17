using AgapeApp.Application.Eventos.DTOs;
using MediatR;

namespace AgapeApp.Application.Eventos.Commands.AbrirEvento;

/// <summary>
/// Abre o evento para operações (somente a partir de Rascunho). Somente administradores.
/// O campo UsuarioId é preenchido pela API com o usuário autenticado.
/// </summary>
public record AbrirEventoCommand(
    Guid Id,
    Guid UsuarioId) : IRequest<EventoDTO>;
