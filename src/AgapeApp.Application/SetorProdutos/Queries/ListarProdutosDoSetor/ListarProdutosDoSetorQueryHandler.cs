using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.EventoProdutos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.SetorProdutos.Queries.ListarProdutosDoSetor;

public class ListarProdutosDoSetorQueryHandler
    : IRequestHandler<ListarProdutosDoSetorQuery, IReadOnlyList<EventoProdutoDTO>>
{
    private readonly ISetorRepository _setorRepository;
    private readonly ISetorProdutoRepository _setorProdutoRepository;
    private readonly IEventoProdutoRepository _eventoProdutoRepository;
    private readonly IProdutoRepository _produtoRepository;

    public ListarProdutosDoSetorQueryHandler(
        ISetorRepository setorRepository,
        ISetorProdutoRepository setorProdutoRepository,
        IEventoProdutoRepository eventoProdutoRepository,
        IProdutoRepository produtoRepository)
    {
        _setorRepository = setorRepository;
        _setorProdutoRepository = setorProdutoRepository;
        _eventoProdutoRepository = eventoProdutoRepository;
        _produtoRepository = produtoRepository;
    }

    public async Task<IReadOnlyList<EventoProdutoDTO>> Handle(
        ListarProdutosDoSetorQuery consulta,
        CancellationToken cancellationToken)
    {
        if (await _setorRepository.ObterPorIdAsync(consulta.SetorId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.SetorNaoEncontrado);

        var vinculos = await _setorProdutoRepository
            .ListarPorSetorAsync(consulta.SetorId, cancellationToken);

        var eventoProdutos = await _eventoProdutoRepository
            .ObterPorIdsAsync(vinculos.Select(vinculo => vinculo.EventoProdutoId), cancellationToken);

        var produtos = await _produtoRepository
            .ObterPorIdsAsync(eventoProdutos.Select(eventoProduto => eventoProduto.ProdutoId), cancellationToken);

        var nomesPorProdutoId = produtos.ToDictionary(produto => produto.Id, produto => produto.Nome);

        return eventoProdutos
            .Select(eventoProduto => eventoProduto.ParaDTO(
                nomesPorProdutoId.GetValueOrDefault(eventoProduto.ProdutoId, string.Empty)))
            .OrderBy(eventoProduto => eventoProduto.ProdutoNome)
            .ToList();
    }
}
