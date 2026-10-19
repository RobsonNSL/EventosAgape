using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Produtos.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.AlterarAtivacaoProduto;

public class AlterarAtivacaoProdutoCommandHandler
    : IRequestHandler<AlterarAtivacaoProdutoCommand, ProdutoDTO>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarAtivacaoProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        IUnitOfWork unitOfWork)
    {
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProdutoDTO> Handle(
        AlterarAtivacaoProdutoCommand comando,
        CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.ProdutoNaoEncontrado);

        if (comando.Ativo)
            produto.Ativar();
        else
            produto.Desativar();

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return produto.ParaDTO();
    }
}
