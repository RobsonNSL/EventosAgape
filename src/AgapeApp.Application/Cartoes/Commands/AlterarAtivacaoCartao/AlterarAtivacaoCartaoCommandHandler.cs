using AgapeApp.Application.Cartoes.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Cartoes.Commands.AlterarAtivacaoCartao;

public class AlterarAtivacaoCartaoCommandHandler
    : IRequestHandler<AlterarAtivacaoCartaoCommand, CartaoDTO>
{
    private readonly ICartaoRepository _cartaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarAtivacaoCartaoCommandHandler(
        ICartaoRepository cartaoRepository,
        IUnitOfWork unitOfWork)
    {
        _cartaoRepository = cartaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartaoDTO> Handle(
        AlterarAtivacaoCartaoCommand comando,
        CancellationToken cancellationToken)
    {
        var cartao = await _cartaoRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.CartaoNaoEncontrado);

        if (comando.Ativo)
            cartao.Ativar();
        else
            cartao.Desativar();

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new CartaoDTO(cartao.Id, cartao.Codigo, cartao.Ativo, cartao.CriadoEm);
    }
}
