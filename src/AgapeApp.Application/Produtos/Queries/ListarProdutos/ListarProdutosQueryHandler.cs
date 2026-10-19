using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Produtos.DTOs;
using MediatR;

namespace AgapeApp.Application.Produtos.Queries.ListarProdutos;

public class ListarProdutosQueryHandler : IRequestHandler<ListarProdutosQuery, IReadOnlyList<ProdutoDTO>>
{
    private readonly IProdutoRepository _produtoRepository;

    public ListarProdutosQueryHandler(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<IReadOnlyList<ProdutoDTO>> Handle(
        ListarProdutosQuery consulta,
        CancellationToken cancellationToken)
    {
        var produtos = await _produtoRepository.ListarAsync(consulta.IncluirInativos, cancellationToken);

        return produtos
            .Select(produto => produto.ParaDTO())
            .ToList();
    }
}
