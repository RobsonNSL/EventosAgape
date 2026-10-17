using AgapeApp.Application.Produtos.DTOs;
using MediatR;

namespace AgapeApp.Application.Produtos.Queries.ObterProduto;

/// <summary>
/// Obtém um produto pelo identificador. Somente administradores.
/// </summary>
public record ObterProdutoQuery(Guid Id) : IRequest<ProdutoDTO>;
