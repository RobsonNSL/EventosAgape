using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Produtos.DTOs;
using AgapeApp.Domain.Entities;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.CriarProduto;

public class CriarProdutoCommandHandler : IRequestHandler<CriarProdutoCommand, ProdutoDTO>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        IUnitOfWork unitOfWork)
    {
        _produtoRepository = produtoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProdutoDTO> Handle(CriarProdutoCommand comando, CancellationToken cancellationToken)
    {
        var produto = Produto.Criar(comando.Nome, comando.PrecoPadrao);

        _produtoRepository.Adicionar(produto);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return produto.ParaDTO();
    }
}
