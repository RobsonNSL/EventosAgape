using AgapeApp.Application.Produtos.DTOs;
using MediatR;

namespace AgapeApp.Application.Produtos.Queries.ListarProdutos;

/// <summary>
/// Lista os produtos do catálogo. Por padrão exibe apenas os ativos. Somente administradores.
/// </summary>
public record ListarProdutosQuery(bool IncluirInativos = false) : IRequest<IReadOnlyList<ProdutoDTO>>;
