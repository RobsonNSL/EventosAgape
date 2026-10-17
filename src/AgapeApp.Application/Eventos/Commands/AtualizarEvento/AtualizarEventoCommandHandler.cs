using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Eventos.Commands.AtualizarEvento;

public class AtualizarEventoCommandHandler : IRequestHandler<AtualizarEventoCommand, EventoDTO>
{
    private readonly IEventoRepository _eventoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarEventoCommandHandler(
        IEventoRepository eventoRepository,
        IUnitOfWork unitOfWork)
    {
        _eventoRepository = eventoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoDTO> Handle(AtualizarEventoCommand comando, CancellationToken cancellationToken)
    {
        var evento = await _eventoRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.Atualizar(comando.Nome, comando.DataInicio, comando.DataFim);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return evento.ParaDTO();
    }
}
