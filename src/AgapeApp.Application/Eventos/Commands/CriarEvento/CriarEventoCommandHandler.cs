using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Eventos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using MediatR;

namespace AgapeApp.Application.Eventos.Commands.CriarEvento;

public class CriarEventoCommandHandler : IRequestHandler<CriarEventoCommand, EventoDTO>
{
    private readonly IEventoRepository _eventoRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public CriarEventoCommandHandler(
        IEventoRepository eventoRepository,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _eventoRepository = eventoRepository;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoDTO> Handle(CriarEventoCommand comando, CancellationToken cancellationToken)
    {
        var evento = Evento.Criar(comando.Nome, comando.DataInicio, comando.DataFim, comando.CriadoPor);

        _eventoRepository.Adicionar(evento);

        _auditoriaService.Registrar(
            comando.CriadoPor,
            "CriarEvento",
            "Evento",
            evento.Id,
            evento.Id,
            new { evento.Nome, evento.DataInicio, evento.DataFim });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return evento.ParaDTO();
    }
}
