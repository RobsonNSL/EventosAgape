using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.EventoOperadores.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.EventoOperadores.Commands.AtualizarDesignacaoOperador;

public class AtualizarDesignacaoOperadorCommandHandler
    : IRequestHandler<AtualizarDesignacaoOperadorCommand, EventoOperadorDTO>
{
    private readonly IEventoOperadorRepository _eventoOperadorRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly ISetorRepository _setorRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarDesignacaoOperadorCommandHandler(
        IEventoOperadorRepository eventoOperadorRepository,
        IEventoRepository eventoRepository,
        ISetorRepository setorRepository,
        IUsuarioRepository usuarioRepository,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _eventoOperadorRepository = eventoOperadorRepository;
        _eventoRepository = eventoRepository;
        _setorRepository = setorRepository;
        _usuarioRepository = usuarioRepository;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoOperadorDTO> Handle(
        AtualizarDesignacaoOperadorCommand comando,
        CancellationToken cancellationToken)
    {
        var designacao = await _eventoOperadorRepository
            .ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.DesignacaoNaoEncontrada);

        var evento = await _eventoRepository.ObterPorIdAsync(designacao.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        var setor = await _setorRepository.ObterPorIdAsync(comando.SetorId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.SetorNaoEncontrado);

        if (setor.EventoId != designacao.EventoId)
            throw new RegraNegocioException(MensagensErro.SetorDeOutroEvento);

        designacao.Atualizar(comando.SetorId, comando.Habilitado);

        _auditoriaService.Registrar(
            comando.SolicitanteId,
            "AtualizarDesignacaoOperador",
            "EventoOperador",
            designacao.Id,
            designacao.EventoId,
            new { comando.SetorId, comando.Habilitado });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        var usuario = await _usuarioRepository.ObterPorIdAsync(designacao.UsuarioId, cancellationToken);

        return new EventoOperadorDTO(
            designacao.Id,
            designacao.EventoId,
            designacao.UsuarioId,
            usuario?.Nome ?? string.Empty,
            usuario?.Email ?? string.Empty,
            setor.Id,
            setor.Nome,
            designacao.Habilitado);
    }
}
