using AgapeApp.Application.EventoProdutos.DTOs;
using MediatR;

namespace AgapeApp.Application.EventoProdutos.Commands.AjustarEstoqueEventoProduto;

/// <summary>
/// Ajuste manual de estoque (positivo para entrada, negativo para baixa).
/// Permitido no meio do evento; não permitido em evento encerrado.
/// O campo UsuarioId é preenchido pela API com o usuário autenticado. Somente administradores.
/// </summary>
public record AjustarEstoqueEventoProdutoCommand(
    Guid Id,
    int Quantidade,
    string? Observacao,
    Guid UsuarioId) : IRequest<EventoProdutoDTO>;
