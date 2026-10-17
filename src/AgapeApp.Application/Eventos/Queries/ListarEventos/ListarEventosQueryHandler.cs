using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Application.Interfaces;
using MediatR;

namespace AgapeApp.Application.Eventos.Queries.ListarEventos;

public class ListarEventosQueryHandler : IRequestHandler<ListarEventosQuery, IReadOnlyList<EventoDTO>>
{
    private readonly IEventoRepository _eventoRepository;

    public ListarEventosQueryHandler(IEventoRepository eventoRepository)
    {
        _eventoRepository = eventoRepository;
    }

    public async Task<IReadOnlyList<EventoDTO>> Handle(
        ListarEventosQuery consulta,
        CancellationToken cancellationToken)
    {
        var eventos = await _eventoRepository.ListarAsync(consulta.Situacao, cancellationToken);

        return eventos.Select(evento => evento.ParaDTO()).ToList();
    }
}
