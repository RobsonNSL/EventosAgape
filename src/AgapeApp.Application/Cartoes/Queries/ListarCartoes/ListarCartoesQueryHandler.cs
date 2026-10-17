using AgapeApp.Application.Cartoes.DTOs;
using AgapeApp.Application.Interfaces;
using MediatR;

namespace AgapeApp.Application.Cartoes.Queries.ListarCartoes;

public class ListarCartoesQueryHandler : IRequestHandler<ListarCartoesQuery, IReadOnlyList<CartaoDTO>>
{
    private readonly ICartaoRepository _cartaoRepository;

    public ListarCartoesQueryHandler(ICartaoRepository cartaoRepository)
    {
        _cartaoRepository = cartaoRepository;
    }

    public async Task<IReadOnlyList<CartaoDTO>> Handle(
        ListarCartoesQuery consulta,
        CancellationToken cancellationToken)
    {
        var cartoes = await _cartaoRepository.ListarAsync(
            consulta.Codigo,
            consulta.IncluirInativos,
            cancellationToken);

        return cartoes
            .Select(cartao => new CartaoDTO(cartao.Id, cartao.Codigo, cartao.Ativo, cartao.CriadoEm))
            .ToList();
    }
}
