using AgapeApp.Application.Produtos.DTOs;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.CriarProduto;

/// <summary>
/// Cadastra um novo produto no catálogo. Somente administradores.
/// </summary>
public record CriarProdutoCommand(
    string Nome,
    decimal? PrecoPadrao) : IRequest<ProdutoDTO>;
