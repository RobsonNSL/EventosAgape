using AgapeApp.Application.Auditoria.DTOs;
using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Auditoria.Queries.ListarAuditoriaDoEvento;

public class ListarAuditoriaDoEventoQueryHandler
    : IRequestHandler<ListarAuditoriaDoEventoQuery, ResultadoPaginadoDTO<LogAuditoriaDTO>>
{
    private const int TamanhoPaginaMaximo = 200;

    private readonly ILogAuditoriaRepository _logAuditoriaRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public ListarAuditoriaDoEventoQueryHandler(
        ILogAuditoriaRepository logAuditoriaRepository,
        IEventoRepository eventoRepository,
        IUsuarioRepository usuarioRepository)
    {
        _logAuditoriaRepository = logAuditoriaRepository;
        _eventoRepository = eventoRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<ResultadoPaginadoDTO<LogAuditoriaDTO>> Handle(
        ListarAuditoriaDoEventoQuery consulta,
        CancellationToken cancellationToken)
    {
        if (await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var pagina = Math.Max(consulta.Pagina, 1);
        var tamanhoPagina = Math.Clamp(consulta.TamanhoPagina, 1, TamanhoPaginaMaximo);

        var logs = await _logAuditoriaRepository.ListarPorEventoAsync(
            consulta.EventoId,
            (pagina - 1) * tamanhoPagina,
            tamanhoPagina,
            cancellationToken);

        var total = await _logAuditoriaRepository.ContarPorEventoAsync(consulta.EventoId, cancellationToken);

        var usuarios = await _usuarioRepository
            .ObterPorIdsAsync(logs.Select(log => log.UsuarioId), cancellationToken);

        var nomesPorUsuarioId = usuarios.ToDictionary(usuario => usuario.Id, usuario => usuario.Nome);

        var itens = logs
            .Select(log => new LogAuditoriaDTO(
                log.Id,
                nomesPorUsuarioId.GetValueOrDefault(log.UsuarioId, string.Empty),
                log.Acao,
                log.Entidade,
                log.EntidadeId,
                log.Dados,
                log.Dispositivo,
                log.CriadoEm))
            .ToList();

        return new ResultadoPaginadoDTO<LogAuditoriaDTO>(itens, total, pagina, tamanhoPagina);
    }
}
