using AgapeApp.Application.Cartoes.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Cartoes.Queries.ObterSaldoCartao;

public class ObterSaldoCartaoQueryHandler : IRequestHandler<ObterSaldoCartaoQuery, CartaoSaldoDTO>
{
    private readonly ICartaoRepository _cartaoRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly ISaldoCartaoEventoRepository _saldoCartaoEventoRepository;

    public ObterSaldoCartaoQueryHandler(
        ICartaoRepository cartaoRepository,
        IEventoRepository eventoRepository,
        ISaldoCartaoEventoRepository saldoCartaoEventoRepository)
    {
        _cartaoRepository = cartaoRepository;
        _eventoRepository = eventoRepository;
        _saldoCartaoEventoRepository = saldoCartaoEventoRepository;
    }

    public async Task<CartaoSaldoDTO> Handle(
        ObterSaldoCartaoQuery consulta,
        CancellationToken cancellationToken)
    {
        var codigoNormalizado = Cartao.NormalizarCodigo(consulta.Codigo);

        var cartao = await _cartaoRepository.ObterPorCodigoAsync(codigoNormalizado, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.CartaoNaoEncontrado);

        if (await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var saldo = await _saldoCartaoEventoRepository
            .ObterAsync(cartao.Id, consulta.EventoId, cancellationToken);

        return new CartaoSaldoDTO(
            cartao.Id,
            cartao.Codigo,
            cartao.Ativo,
            consulta.EventoId,
            saldo?.Saldo ?? 0m);
    }
}
