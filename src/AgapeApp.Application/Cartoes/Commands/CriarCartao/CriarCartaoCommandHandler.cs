using AgapeApp.Application.Cartoes.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Cartoes.Commands.CriarCartao;

public class CriarCartaoCommandHandler : IRequestHandler<CriarCartaoCommand, CartaoDTO>
{
    private readonly ICartaoRepository _cartaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarCartaoCommandHandler(
        ICartaoRepository cartaoRepository,
        IUnitOfWork unitOfWork)
    {
        _cartaoRepository = cartaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartaoDTO> Handle(CriarCartaoCommand comando, CancellationToken cancellationToken)
    {
        var codigoNormalizado = Cartao.NormalizarCodigo(comando.Codigo);

        if (await _cartaoRepository.ExistePorCodigoAsync(codigoNormalizado, cancellationToken))
            throw new ConflitoException(MensagensErro.CartaoCodigoDuplicado);

        var cartao = Cartao.Criar(codigoNormalizado);

        _cartaoRepository.Adicionar(cartao);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new CartaoDTO(cartao.Id, cartao.Codigo, cartao.Ativo, cartao.CriadoEm);
    }
}
