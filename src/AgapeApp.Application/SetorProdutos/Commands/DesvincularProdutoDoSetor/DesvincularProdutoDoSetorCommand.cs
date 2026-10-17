using MediatR;

namespace AgapeApp.Application.SetorProdutos.Commands.DesvincularProdutoDoSetor;

/// <summary>
/// Remove o vínculo de um produto com um setor (o produto deixa de ser vendido no setor).
/// Não permitido em evento encerrado. Somente administradores.
/// </summary>
public record DesvincularProdutoDoSetorCommand(
    Guid SetorId,
    Guid EventoProdutoId) : IRequest;
