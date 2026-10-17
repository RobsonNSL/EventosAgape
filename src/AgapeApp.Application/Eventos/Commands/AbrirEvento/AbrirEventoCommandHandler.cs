using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Eventos.Commands.AbrirEvento;

public class AbrirEventoCommandHandler : IRequestHandler<AbrirEventoCommand, EventoDTO>
{
    private readonly IEventoRepository _eventoRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public AbrirEventoCommandHandler(
        IEventoRepository eventoRepository,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _eventoRepository = eventoRepository;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoDTO> Handle(AbrirEventoCommand comando, CancellationToken cancellationToken)
    {
        var evento = await _eventoRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.Abrir();

        _auditoriaService.Registrar(
            comando.UsuarioId,
            "AbrirEvento",
            "Evento",
            evento.Id,
            evento.Id,
            new { situacao = evento.Situacao.ToString() });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return evento.ParaDTO();
    }
}
