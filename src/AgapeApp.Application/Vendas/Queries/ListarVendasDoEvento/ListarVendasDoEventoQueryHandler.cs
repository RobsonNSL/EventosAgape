using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Vendas.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Vendas.Queries.ListarVendasDoEvento;

public class ListarVendasDoEventoQueryHandler
    : IRequestHandler<ListarVendasDoEventoQuery, ResultadoPaginadoDTO<VendaDTO>>
{
    private const int TamanhoPaginaMaximo = 200;

    private readonly IVendaRepository _vendaRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly ICartaoRepository _cartaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISetorRepository _setorRepository;

    public ListarVendasDoEventoQueryHandler(
        IVendaRepository vendaRepository,
        IEventoRepository eventoRepository,
        ICartaoRepository cartaoRepository,
        IUsuarioRepository usuarioRepository,
        ISetorRepository setorRepository)
    {
        _vendaRepository = vendaRepository;
        _eventoRepository = eventoRepository;
        _cartaoRepository = cartaoRepository;
        _usuarioRepository = usuarioRepository;
        _setorRepository = setorRepository;
    }

    public async Task<ResultadoPaginadoDTO<VendaDTO>> Handle(
        ListarVendasDoEventoQuery consulta,
        CancellationToken cancellationToken)
    {
        if (await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var pagina = Math.Max(consulta.Pagina, 1);
        var tamanhoPagina = Math.Clamp(consulta.TamanhoPagina, 1, TamanhoPaginaMaximo);

        var vendas = await _vendaRepository.ListarPorEventoAsync(
            consulta.EventoId,
            (pagina - 1) * tamanhoPagina,
            tamanhoPagina,
            cancellationToken);

        var total = await _vendaRepository.ContarPorEventoAsync(consulta.EventoId, cancellationToken);

        var cartoes = await _cartaoRepository
            .ObterPorIdsAsync(vendas.Select(venda => venda.CartaoId), cancellationToken);

        var usuarios = await _usuarioRepository
            .ObterPorIdsAsync(vendas.Select(venda => venda.UsuarioId), cancellationToken);

        var setores = await _setorRepository
            .ObterPorIdsAsync(vendas.Select(venda => venda.SetorId), cancellationToken);

        var itens = await _vendaRepository
            .ListarItensPorVendasAsync(vendas.Select(venda => venda.Id), cancellationToken);

        var cartoesPorId = cartoes.ToDictionary(cartao => cartao.Id, cartao => cartao.Codigo);
        var usuariosPorId = usuarios.ToDictionary(usuario => usuario.Id, usuario => usuario.Nome);
        var setoresPorId = setores.ToDictionary(setor => setor.Id, setor => setor.Nome);
        var quantidadeItensPorVenda = itens
            .GroupBy(item => item.VendaId)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => item.Quantidade));

        var itensResposta = vendas
            .Select(venda => new VendaDTO(
                venda.Id,
                cartoesPorId.GetValueOrDefault(venda.CartaoId, string.Empty),
                setoresPorId.GetValueOrDefault(venda.SetorId, string.Empty),
                usuariosPorId.GetValueOrDefault(venda.UsuarioId, string.Empty),
                venda.Total,
                quantidadeItensPorVenda.GetValueOrDefault(venda.Id, 0),
                venda.CriadoEm))
            .ToList();

        return new ResultadoPaginadoDTO<VendaDTO>(itensResposta, total, pagina, tamanhoPagina);
    }
}
