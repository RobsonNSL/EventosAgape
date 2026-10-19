using AgapeApp.Application.Produtos.DTOs;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.RemoverImagemProduto;

/// <summary>Remove a imagem de um produto do catálogo. Somente administradores.</summary>
public record RemoverImagemProdutoCommand(Guid ProdutoId) : IRequest<ProdutoDTO>;
