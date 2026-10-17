using AgapeApp.Application.Recargas.DTOs;
using MediatR;

namespace AgapeApp.Application.Recargas.Commands.EfetuarRecarga;

/// <summary>
/// Registra uma recarga de cartão no evento aberto em que o operador está trabalhando.
/// O campo IdOperacaoCliente (gerado pelo app) garante a idempotência:
/// retry ou toque duplo não duplica a recarga — a API devolve a mesma recarga.
/// O campo UsuarioId é preenchido pela API com o usuário autenticado.
/// </summary>
public record EfetuarRecargaCommand(
    string CartaoCodigo,
    Guid EventoId,
    Guid SetorId,
    decimal Valor,
    Guid IdOperacaoCliente,
    string? Dispositivo,
    Guid UsuarioId) : IRequest<RecargaResponse>;
