using AgapeApp.Application.Produtos.DTOs;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.AlterarAtivacaoProduto;

/// <summary>
/// Ativa ou desativa um produto (exclusão lógica: nada é apagado de verdade).
/// Somente administradores.
/// </summary>
public record AlterarAtivacaoProdutoCommand(
    Guid Id,
    bool Ativo) : IRequest<ProdutoDTO>;
