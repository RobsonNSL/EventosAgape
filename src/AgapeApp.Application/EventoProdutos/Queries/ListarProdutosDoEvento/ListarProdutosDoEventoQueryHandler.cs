using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.EventoProdutos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.EventoProdutos.Queries.ListarProdutosDoEvento;

public class ListarProdutosDoEventoQueryHandler
    : IRequestHandler<ListarProdutosDoEventoQuery, IReadOnlyList<EventoProdutoDTO>>
{
    private readonly IEventoProdutoRepository _eventoProdutoRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IProdutoRepository _produtoRepository;

    public ListarProdutosDoEventoQueryHandler(
        IEventoProdutoRepository eventoProdutoRepository,
        IEventoRepository eventoRepository,
        IProdutoRepository produtoRepository)
    {
        _eventoProdutoRepository = eventoProdutoRepository;
        _eventoRepository = eventoRepository;
        _produtoRepository = produtoRepository;
    }

    public async Task<IReadOnlyList<EventoProdutoDTO>> Handle(
        ListarProdutosDoEventoQuery consulta,
        CancellationToken cancellationToken)
    {
        if (await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var vinculos = await _eventoProdutoRepository
            .ListarPorEventoAsync(consulta.EventoId, cancellationToken);

        var produtos = await _produtoRepository
            .ObterPorIdsAsync(vinculos.Select(vinculo => vinculo.ProdutoId), cancellationToken);

        var produtosPorId = produtos.ToDictionary(produto => produto.Id);

        return vinculos
            .Select(vinculo => vinculo.ParaDTO(produtosPorId.GetValueOrDefault(vinculo.ProdutoId)))
            .OrderBy(eventoProduto => eventoProduto.ProdutoNome)
            .ToList();
    }
}
