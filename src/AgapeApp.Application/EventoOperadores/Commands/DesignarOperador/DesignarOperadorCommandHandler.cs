using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.EventoOperadores.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.EventoOperadores.Commands.DesignarOperador;

public class DesignarOperadorCommandHandler
    : IRequestHandler<DesignarOperadorCommand, EventoOperadorDTO>
{
    private readonly IEventoRepository _eventoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISetorRepository _setorRepository;
    private readonly IEventoOperadorRepository _eventoOperadorRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public DesignarOperadorCommandHandler(
        IEventoRepository eventoRepository,
        IUsuarioRepository usuarioRepository,
        ISetorRepository setorRepository,
        IEventoOperadorRepository eventoOperadorRepository,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _eventoRepository = eventoRepository;
        _usuarioRepository = usuarioRepository;
        _setorRepository = setorRepository;
        _eventoOperadorRepository = eventoOperadorRepository;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoOperadorDTO> Handle(
        DesignarOperadorCommand comando,
        CancellationToken cancellationToken)
    {
        var evento = await _eventoRepository.ObterPorIdAsync(comando.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        var usuario = await _usuarioRepository.ObterPorIdAsync(comando.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        // Operadores e administradores podem trabalhar no evento (o admin também se designa).
        if (!usuario.Ativo)
            throw new RegraNegocioException(MensagensErro.UsuarioDesativado);

        var setor = await _setorRepository.ObterPorIdAsync(comando.SetorId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.SetorNaoEncontrado);

        if (setor.EventoId != comando.EventoId)
            throw new RegraNegocioException(MensagensErro.SetorDeOutroEvento);

        var existente = await _eventoOperadorRepository
            .ObterPorEventoEUsuarioAsync(comando.EventoId, comando.UsuarioId, cancellationToken);

        if (existente is not null)
            throw new ConflitoException(MensagensErro.OperadorJaDesignado);

        var designacao = EventoOperador.Criar(comando.EventoId, comando.UsuarioId, comando.SetorId);

        _eventoOperadorRepository.Adicionar(designacao);

        _auditoriaService.Registrar(
            comando.SolicitanteId,
            "DesignarOperador",
            "EventoOperador",
            designacao.Id,
            comando.EventoId,
            new { comando.UsuarioId, comando.SetorId });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new EventoOperadorDTO(
            designacao.Id,
            designacao.EventoId,
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            setor.Id,
            setor.Nome,
            designacao.Habilitado);
    }
}
