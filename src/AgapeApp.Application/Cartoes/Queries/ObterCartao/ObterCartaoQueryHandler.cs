using AgapeApp.Application.Cartoes.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Cartoes.Queries.ObterCartao;

public class ObterCartaoQueryHandler : IRequestHandler<ObterCartaoQuery, CartaoDTO>
{
    private readonly ICartaoRepository _cartaoRepository;

    public ObterCartaoQueryHandler(ICartaoRepository cartaoRepository)
    {
        _cartaoRepository = cartaoRepository;
    }

    public async Task<CartaoDTO> Handle(ObterCartaoQuery consulta, CancellationToken cancellationToken)
    {
        var cartao = await _cartaoRepository.ObterPorIdAsync(consulta.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.CartaoNaoEncontrado);

        return new CartaoDTO(cartao.Id, cartao.Codigo, cartao.Ativo, cartao.CriadoEm);
    }
}
