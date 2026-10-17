using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.EventoProdutos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.EventoProdutos.Commands.AtualizarEventoProduto;

public class AtualizarEventoProdutoCommandHandler
    : IRequestHandler<AtualizarEventoProdutoCommand, EventoProdutoDTO>
{
    private readonly IEventoProdutoRepository _eventoProdutoRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarEventoProdutoCommandHandler(
        IEventoProdutoRepository eventoProdutoRepository,
        IEventoRepository eventoRepository,
        IProdutoRepository produtoRepository,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _eventoProdutoRepository = eventoProdutoRepository;
        _eventoRepository = eventoRepository;
        _produtoRepository = produtoRepository;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoProdutoDTO> Handle(
        AtualizarEventoProdutoCommand comando,
        CancellationToken cancellationToken)
    {
        var eventoProduto = await _eventoProdutoRepository
            .ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoProdutoNaoEncontrado);

        var evento = await _eventoRepository.ObterPorIdAsync(eventoProduto.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        var produto = await _produtoRepository.ObterPorIdAsync(eventoProduto.ProdutoId, cancellationToken);

        eventoProduto.Atualizar(comando.Preco, comando.ControlaEstoque);

        _auditoriaService.Registrar(
            comando.UsuarioId,
            "AtualizarEventoProduto",
            "EventoProduto",
            eventoProduto.Id,
            eventoProduto.EventoId,
            new { comando.Preco, comando.ControlaEstoque });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return eventoProduto.ParaDTO(produto?.Nome ?? string.Empty);
    }
}
