using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Produtos.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.AtualizarProduto;

public class AtualizarProdutoCommandHandler : IRequestHandler<AtualizarProdutoCommand, ProdutoDTO>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        IUnitOfWork unitOfWork)
    {
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProdutoDTO> Handle(AtualizarProdutoCommand comando, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.ProdutoNaoEncontrado);

        produto.Atualizar(comando.Nome, comando.PrecoPadrao);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return produto.ParaDTO();
    }
}
