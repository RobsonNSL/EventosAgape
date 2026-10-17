using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Recargas.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Recargas.Queries.ListarRecargas;

public class ListarRecargasQueryHandler
    : IRequestHandler<ListarRecargasQuery, ResultadoPaginadoDTO<RecargaDTO>>
{
    private const int TamanhoPaginaMaximo = 200;

    private readonly IRecargaRepository _recargaRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly ICartaoRepository _cartaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISetorRepository _setorRepository;

    public ListarRecargasQueryHandler(
        IRecargaRepository recargaRepository,
        IEventoRepository eventoRepository,
        ICartaoRepository cartaoRepository,
        IUsuarioRepository usuarioRepository,
        ISetorRepository setorRepository)
    {
        _recargaRepository = recargaRepository;
        _eventoRepository = eventoRepository;
        _cartaoRepository = cartaoRepository;
        _usuarioRepository = usuarioRepository;
        _setorRepository = setorRepository;
    }

    public async Task<ResultadoPaginadoDTO<RecargaDTO>> Handle(
        ListarRecargasQuery consulta,
        CancellationToken cancellationToken)
    {
        if (await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var pagina = Math.Max(consulta.Pagina, 1);
        var tamanhoPagina = Math.Clamp(consulta.TamanhoPagina, 1, TamanhoPaginaMaximo);

        var recargas = await _recargaRepository.ListarPorEventoAsync(
            consulta.EventoId,
            (pagina - 1) * tamanhoPagina,
            tamanhoPagina,
            cancellationToken);

        var total = await _recargaRepository.ContarPorEventoAsync(consulta.EventoId, cancellationToken);

        var cartoes = await _cartaoRepository
            .ObterPorIdsAsync(recargas.Select(recarga => recarga.CartaoId), cancellationToken);

        var usuarios = await _usuarioRepository
            .ObterPorIdsAsync(recargas.Select(recarga => recarga.UsuarioId), cancellationToken);

        var setores = await _setorRepository
            .ObterPorIdsAsync(recargas.Select(recarga => recarga.SetorId), cancellationToken);

        var cartoesPorId = cartoes.ToDictionary(cartao => cartao.Id, cartao => cartao.Codigo);
        var usuariosPorId = usuarios.ToDictionary(usuario => usuario.Id, usuario => usuario.Nome);
        var setoresPorId = setores.ToDictionary(setor => setor.Id, setor => setor.Nome);

        var itens = recargas
            .Select(recarga => new RecargaDTO(
                recarga.Id,
                cartoesPorId.GetValueOrDefault(recarga.CartaoId, string.Empty),
                recarga.Valor,
                usuariosPorId.GetValueOrDefault(recarga.UsuarioId, string.Empty),
                setoresPorId.GetValueOrDefault(recarga.SetorId, string.Empty),
                recarga.Dispositivo,
                recarga.CriadoEm))
            .ToList();

        return new ResultadoPaginadoDTO<RecargaDTO>(itens, total, pagina, tamanhoPagina);
    }
}
