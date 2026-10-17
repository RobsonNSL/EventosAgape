using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Relatorios.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Relatorios.Queries.ListarSaldosDoEvento;

public class ListarSaldosDoEventoQueryHandler
    : IRequestHandler<ListarSaldosDoEventoQuery, IReadOnlyList<SaldoEmCirculacaoDTO>>
{
    private readonly ISaldoCartaoEventoRepository _saldoCartaoEventoRepository;
    private readonly ICartaoRepository _cartaoRepository;
    private readonly IEventoRepository _eventoRepository;

    public ListarSaldosDoEventoQueryHandler(
        ISaldoCartaoEventoRepository saldoCartaoEventoRepository,
        ICartaoRepository cartaoRepository,
        IEventoRepository eventoRepository)
    {
        _saldoCartaoEventoRepository = saldoCartaoEventoRepository;
        _cartaoRepository = cartaoRepository;
        _eventoRepository = eventoRepository;
    }

    public async Task<IReadOnlyList<SaldoEmCirculacaoDTO>> Handle(
        ListarSaldosDoEventoQuery consulta,
        CancellationToken cancellationToken)
    {
        if (await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var saldos = await _saldoCartaoEventoRepository
            .ListarPorEventoAsync(consulta.EventoId, cancellationToken);

        var cartoes = await _cartaoRepository
            .ObterPorIdsAsync(saldos.Select(saldo => saldo.CartaoId), cancellationToken);

        var codigosPorCartaoId = cartoes.ToDictionary(cartao => cartao.Id, cartao => cartao.Codigo);

        return saldos
            .OrderByDescending(saldo => saldo.Saldo)
            .Select(saldo => new SaldoEmCirculacaoDTO(
                saldo.CartaoId,
                codigosPorCartaoId.GetValueOrDefault(saldo.CartaoId, string.Empty),
                saldo.Saldo))
            .ToList();
    }
}
