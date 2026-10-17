using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Vendas.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Vendas.Queries.ObterVenda;

public class ObterVendaQueryHandler : IRequestHandler<ObterVendaQuery, VendaDetalheDTO>
{
    private readonly IVendaRepository _vendaRepository;
    private readonly ICartaoRepository _cartaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISetorRepository _setorRepository;

    public ObterVendaQueryHandler(
        IVendaRepository vendaRepository,
        ICartaoRepository cartaoRepository,
        IUsuarioRepository usuarioRepository,
        ISetorRepository setorRepository)
    {
        _vendaRepository = vendaRepository;
        _cartaoRepository = cartaoRepository;
        _usuarioRepository = usuarioRepository;
        _setorRepository = setorRepository;
    }

    public async Task<VendaDetalheDTO> Handle(
        ObterVendaQuery consulta,
        CancellationToken cancellationToken)
    {
        var venda = await _vendaRepository.ObterPorIdAsync(consulta.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.VendaNaoEncontrada);

        var cartao = await _cartaoRepository.ObterPorIdAsync(venda.CartaoId, cancellationToken);

        var usuario = await _usuarioRepository.ObterPorIdAsync(venda.UsuarioId, cancellationToken);

        var setor = await _setorRepository.ObterPorIdAsync(venda.SetorId, cancellationToken);

        var itens = await _vendaRepository.ListarItensPorVendaAsync(venda.Id, cancellationToken);

        var itensResposta = itens
            .Select(item => new ItemVendaDTO(
                item.EventoProdutoId,
                item.ProdutoNome,
                item.PrecoUnitario,
                item.Quantidade,
                item.Total))
            .ToList();

        return new VendaDetalheDTO(
            venda.Id,
            cartao?.Codigo ?? string.Empty,
            venda.EventoId,
            setor?.Nome ?? string.Empty,
            usuario?.Nome ?? string.Empty,
            venda.Total,
            venda.Dispositivo,
            itensResposta,
            venda.CriadoEm);
    }
}
