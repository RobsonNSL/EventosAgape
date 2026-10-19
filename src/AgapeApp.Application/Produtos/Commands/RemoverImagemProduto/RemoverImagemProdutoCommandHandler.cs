using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Produtos.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.RemoverImagemProduto;

public class RemoverImagemProdutoCommandHandler : IRequestHandler<RemoverImagemProdutoCommand, ProdutoDTO>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IArmazenamentoImagens _armazenamentoImagens;
    private readonly IUnitOfWork _unitOfWork;

    public RemoverImagemProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        IArmazenamentoImagens armazenamentoImagens,
        IUnitOfWork unitOfWork)
    {
        _produtoRepository = produtoRepository;
        _armazenamentoImagens = armazenamentoImagens;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProdutoDTO> Handle(RemoverImagemProdutoCommand comando, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.ObterParaAtualizarAsync(comando.ProdutoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.ProdutoNaoEncontrado);

        var imagemAnterior = produto.CaminhoImagem;

        produto.DefinirImagem(null);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        _armazenamentoImagens.Remover(imagemAnterior);

        return produto.ParaDTO();
    }
}
