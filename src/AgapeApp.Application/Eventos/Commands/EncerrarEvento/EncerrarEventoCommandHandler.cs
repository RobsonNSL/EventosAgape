using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Eventos.Commands.EncerrarEvento;

public class EncerrarEventoCommandHandler : IRequestHandler<EncerrarEventoCommand, EventoDTO>
{
    private readonly IEventoRepository _eventoRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public EncerrarEventoCommandHandler(
        IEventoRepository eventoRepository,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _eventoRepository = eventoRepository;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoDTO> Handle(EncerrarEventoCommand comando, CancellationToken cancellationToken)
    {
        var evento = await _eventoRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.Encerrar();

        _auditoriaService.Registrar(
            comando.UsuarioId,
            "EncerrarEvento",
            "Evento",
            evento.Id,
            evento.Id,
            new { situacao = evento.Situacao.ToString() });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return evento.ParaDTO();
    }
}
