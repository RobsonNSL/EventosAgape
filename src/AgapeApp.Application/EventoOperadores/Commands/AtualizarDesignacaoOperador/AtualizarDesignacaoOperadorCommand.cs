using AgapeApp.Application.EventoOperadores.DTOs;
using MediatR;

namespace AgapeApp.Application.EventoOperadores.Commands.AtualizarDesignacaoOperador;

/// <summary>
/// Troca o setor designado e/ou habilita/desabilita o operador no evento. Somente administradores.
/// O campo SolicitanteId é preenchido pela API com o usuário autenticado.
/// </summary>
public record AtualizarDesignacaoOperadorCommand(
    Guid Id,
    Guid SetorId,
    bool Habilitado,
    Guid SolicitanteId) : IRequest<EventoOperadorDTO>;
