using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.EventoOperadores.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.EventoOperadores.Queries.ListarOperadoresDoEvento;

public class ListarOperadoresDoEventoQueryHandler
    : IRequestHandler<ListarOperadoresDoEventoQuery, IReadOnlyList<EventoOperadorDTO>>
{
    private readonly IEventoOperadorRepository _eventoOperadorRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISetorRepository _setorRepository;

    public ListarOperadoresDoEventoQueryHandler(
        IEventoOperadorRepository eventoOperadorRepository,
        IEventoRepository eventoRepository,
        IUsuarioRepository usuarioRepository,
        ISetorRepository setorRepository)
    {
        _eventoOperadorRepository = eventoOperadorRepository;
        _eventoRepository = eventoRepository;
        _usuarioRepository = usuarioRepository;
        _setorRepository = setorRepository;
    }

    public async Task<IReadOnlyList<EventoOperadorDTO>> Handle(
        ListarOperadoresDoEventoQuery consulta,
        CancellationToken cancellationToken)
    {
        if (await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var designacoes = await _eventoOperadorRepository
            .ListarPorEventoAsync(consulta.EventoId, cancellationToken);

        var usuarios = await _usuarioRepository
            .ObterPorIdsAsync(designacoes.Select(designacao => designacao.UsuarioId), cancellationToken);

        var setores = await _setorRepository
            .ListarPorEventoAsync(consulta.EventoId, cancellationToken);

        var usuariosPorId = usuarios.ToDictionary(usuario => usuario.Id);
        var setoresPorId = setores.ToDictionary(setor => setor.Id);

        return designacoes
            .Select(designacao =>
            {
                usuariosPorId.TryGetValue(designacao.UsuarioId, out var usuario);
                setoresPorId.TryGetValue(designacao.SetorId, out var setor);

                return new EventoOperadorDTO(
                    designacao.Id,
                    designacao.EventoId,
                    designacao.UsuarioId,
                    usuario?.Nome ?? string.Empty,
                    usuario?.Email ?? string.Empty,
                    designacao.SetorId,
                    setor?.Nome ?? string.Empty,
                    designacao.Habilitado);
            })
            .OrderBy(designacao => designacao.UsuarioNome)
            .ToList();
    }
}
