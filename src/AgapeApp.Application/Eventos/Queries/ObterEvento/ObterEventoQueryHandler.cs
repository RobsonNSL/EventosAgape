using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Eventos.Queries.ObterEvento;

public class ObterEventoQueryHandler : IRequestHandler<ObterEventoQuery, EventoDTO>
{
    private readonly IEventoRepository _eventoRepository;

    public ObterEventoQueryHandler(IEventoRepository eventoRepository)
    {
        _eventoRepository = eventoRepository;
    }

    public async Task<EventoDTO> Handle(ObterEventoQuery consulta, CancellationToken cancellationToken)
    {
        var evento = await _eventoRepository.ObterPorIdAsync(consulta.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        return evento.ParaDTO();
    }
}
