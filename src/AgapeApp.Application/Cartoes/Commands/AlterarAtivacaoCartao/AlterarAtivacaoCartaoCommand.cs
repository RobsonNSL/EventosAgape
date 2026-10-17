using AgapeApp.Application.Cartoes.DTOs;
using MediatR;

namespace AgapeApp.Application.Cartoes.Commands.AlterarAtivacaoCartao;

/// <summary>
/// Ativa ou desativa um cartão (exclusão lógica: nada é apagado de verdade).
/// Somente administradores.
/// </summary>
public record AlterarAtivacaoCartaoCommand(
    Guid Id,
    bool Ativo) : IRequest<CartaoDTO>;
