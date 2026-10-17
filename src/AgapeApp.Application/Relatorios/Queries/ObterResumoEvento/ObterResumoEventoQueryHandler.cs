using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Relatorios.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Relatorios.Queries.ObterResumoEvento;

public class ObterResumoEventoQueryHandler : IRequestHandler<ObterResumoEventoQuery, ResumoEventoDTO>
{
    private readonly IEventoRepository _eventoRepository;
    private readonly IRecargaRepository _recargaRepository;
    private readonly IVendaRepository _vendaRepository;
    private readonly ISaldoCartaoEventoRepository _saldoCartaoEventoRepository;
    private readonly ISetorRepository _setorRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public ObterResumoEventoQueryHandler(
        IEventoRepository eventoRepository,
        IRecargaRepository recargaRepository,
        IVendaRepository vendaRepository,
        ISaldoCartaoEventoRepository saldoCartaoEventoRepository,
        ISetorRepository setorRepository,
        IUsuarioRepository usuarioRepository)
    {
        _eventoRepository = eventoRepository;
        _recargaRepository = recargaRepository;
        _vendaRepository = vendaRepository;
        _saldoCartaoEventoRepository = saldoCartaoEventoRepository;
        _setorRepository = setorRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<ResumoEventoDTO> Handle(
        ObterResumoEventoQuery consulta,
        CancellationToken cancellationToken)
    {
        var evento = await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var recargas = await _recargaRepository.ListarTodasPorEventoAsync(evento.Id, cancellationToken);
        var vendas = await _vendaRepository.ListarTodasPorEventoAsync(evento.Id, cancellationToken);
        var itensVenda = await _vendaRepository.ListarTodosItensPorEventoAsync(evento.Id, cancellationToken);
        var saldos = await _saldoCartaoEventoRepository.ListarPorEventoAsync(evento.Id, cancellationToken);

        var idsDeSetores = recargas.Select(recarga => recarga.SetorId)
            .Concat(vendas.Select(venda => venda.SetorId))
            .Distinct()
            .ToList();

        var idsDeUsuarios = recargas.Select(recarga => recarga.UsuarioId)
            .Concat(vendas.Select(venda => venda.UsuarioId))
            .Distinct()
            .ToList();

        var setores = await _setorRepository.ObterPorIdsAsync(idsDeSetores, cancellationToken);
        var usuarios = await _usuarioRepository.ObterPorIdsAsync(idsDeUsuarios, cancellationToken);

        var setoresPorId = setores.ToDictionary(setor => setor.Id, setor => setor.Nome);
        var usuariosPorId = usuarios.ToDictionary(usuario => usuario.Id, usuario => usuario.Nome);

        var porSetor = idsDeSetores
            .Select(setorId => new ResumoPorSetorDTO(
                setorId,
                setoresPorId.GetValueOrDefault(setorId, string.Empty),
                recargas.Count(recarga => recarga.SetorId == setorId),
                recargas.Where(recarga => recarga.SetorId == setorId).Sum(recarga => recarga.Valor),
                vendas.Count(venda => venda.SetorId == setorId),
                vendas.Where(venda => venda.SetorId == setorId).Sum(venda => venda.Total)))
            .OrderByDescending(item => item.TotalRecargas + item.TotalVendas)
            .ToList();

        var porOperador = idsDeUsuarios
            .Select(usuarioId => new ResumoPorOperadorDTO(
                usuarioId,
                usuariosPorId.GetValueOrDefault(usuarioId, string.Empty),
                recargas.Count(recarga => recarga.UsuarioId == usuarioId),
                recargas.Where(recarga => recarga.UsuarioId == usuarioId).Sum(recarga => recarga.Valor),
                vendas.Count(venda => venda.UsuarioId == usuarioId),
                vendas.Where(venda => venda.UsuarioId == usuarioId).Sum(venda => venda.Total)))
            .OrderByDescending(item => item.TotalRecargas + item.TotalVendas)
            .ToList();

        var porProduto = itensVenda
            .GroupBy(item => new { item.EventoProdutoId, item.ProdutoNome })
            .Select(grupo => new ResumoPorProdutoDTO(
                grupo.Key.EventoProdutoId,
                grupo.Key.ProdutoNome,
                grupo.Sum(item => item.Quantidade),
                grupo.Sum(item => item.Total)))
            .OrderByDescending(item => item.TotalVendido)
            .ToList();

        return new ResumoEventoDTO(
            evento.Id,
            evento.Nome,
            evento.Situacao.ToString(),
            recargas.Count,
            recargas.Sum(recarga => recarga.Valor),
            vendas.Count,
            vendas.Sum(venda => venda.Total),
            saldos.Sum(saldo => saldo.Saldo),
            porSetor,
            porOperador,
            porProduto);
    }
}
