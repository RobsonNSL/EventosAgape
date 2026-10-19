using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Produtos.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Produtos.Queries.ObterProduto;

public class ObterProdutoQueryHandler : IRequestHandler<ObterProdutoQuery, ProdutoDTO>
{
    private readonly IProdutoRepository _produtoRepository;

    public ObterProdutoQueryHandler(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<ProdutoDTO> Handle(ObterProdutoQuery consulta, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(consulta.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.ProdutoNaoEncontrado);

        return produto.ParaDTO();
    }
}
