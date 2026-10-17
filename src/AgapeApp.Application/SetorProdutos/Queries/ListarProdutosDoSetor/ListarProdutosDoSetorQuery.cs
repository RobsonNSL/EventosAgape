using AgapeApp.Application.EventoProdutos.DTOs;
using MediatR;

namespace AgapeApp.Application.SetorProdutos.Queries.ListarProdutosDoSetor;

/// <summary>
/// Lista os produtos que um setor vende (usado pelos operadores na tela de venda).
/// </summary>
public record ListarProdutosDoSetorQuery(Guid SetorId) : IRequest<IReadOnlyList<EventoProdutoDTO>>;
