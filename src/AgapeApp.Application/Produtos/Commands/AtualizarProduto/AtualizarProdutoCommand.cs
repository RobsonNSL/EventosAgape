using AgapeApp.Application.Produtos.DTOs;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.AtualizarProduto;

/// <summary>
/// Atualiza nome e preço padrão de um produto. Somente administradores.
/// </summary>
public record AtualizarProdutoCommand(
    Guid Id,
    string Nome,
    decimal? PrecoPadrao) : IRequest<ProdutoDTO>;
