using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Application.Interfaces;
using MediatR;

namespace AgapeApp.Application.Eventos.Queries.ListarMeusEventos;

public class ListarMeusEventosQueryHandler
    : IRequestHandler<ListarMeusEventosQuery, IReadOnlyList<MeuEventoDTO>>
{
    private readonly IEventoOperadorRepository _eventoOperadorRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly ISetorRepository _setorRepository;

    public ListarMeusEventosQueryHandler(
        IEventoOperadorRepository eventoOperadorRepository,
        IEventoRepository eventoRepository,
        ISetorRepository setorRepository)
    {
        _eventoOperadorRepository = eventoOperadorRepository;
        _eventoRepository = eventoRepository;
        _setorRepository = setorRepository;
    }

    public async Task<IReadOnlyList<MeuEventoDTO>> Handle(
        ListarMeusEventosQuery consulta,
        CancellationToken cancellationToken)
    {
        var designacoes = await _eventoOperadorRepository
            .ListarPorUsuarioAsync(consulta.UsuarioId, cancellationToken);

        if (designacoes.Count == 0)
            return [];

        var eventos = await _eventoRepository
            .ObterPorIdsAsync(designacoes.Select(designacao => designacao.EventoId), cancellationToken);

        var setores = await _setorRepository
            .ObterPorIdsAsync(designacoes.Select(designacao => designacao.SetorId), cancellationToken);

        var eventosPorId = eventos.ToDictionary(evento => evento.Id);
        var setoresPorId = setores.ToDictionary(setor => setor.Id);

        return designacoes
            .Where(designacao => eventosPorId.ContainsKey(designacao.EventoId))
            .Select(designacao =>
            {
                var evento = eventosPorId[designacao.EventoId];
                var nomeSetor = setoresPorId.TryGetValue(designacao.SetorId, out var setor)
                    ? setor.Nome
                    : string.Empty;

                return new MeuEventoDTO(
                    evento.Id,
                    evento.Nome,
                    evento.DataInicio,
                    evento.DataFim,
                    evento.Situacao.ToString(),
                    designacao.SetorId,
                    nomeSetor,
                    designacao.Habilitado);
            })
            .OrderByDescending(meuEvento => meuEvento.DataInicio)
            .ThenBy(meuEvento => meuEvento.Nome)
            .ToList();
    }
}
