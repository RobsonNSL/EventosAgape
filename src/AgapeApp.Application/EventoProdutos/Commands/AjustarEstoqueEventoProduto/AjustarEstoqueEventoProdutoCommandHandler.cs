using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.EventoProdutos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.EventoProdutos.Commands.AjustarEstoqueEventoProduto;

public class AjustarEstoqueEventoProdutoCommandHandler
    : IRequestHandler<AjustarEstoqueEventoProdutoCommand, EventoProdutoDTO>
{
    private readonly IEventoProdutoRepository _eventoProdutoRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMovimentoEstoqueRepository _movimentoEstoqueRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public AjustarEstoqueEventoProdutoCommandHandler(
        IEventoProdutoRepository eventoProdutoRepository,
        IEventoRepository eventoRepository,
        IProdutoRepository produtoRepository,
        IMovimentoEstoqueRepository movimentoEstoqueRepository,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _eventoProdutoRepository = eventoProdutoRepository;
        _eventoRepository = eventoRepository;
        _produtoRepository = produtoRepository;
        _movimentoEstoqueRepository = movimentoEstoqueRepository;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoProdutoDTO> Handle(
        AjustarEstoqueEventoProdutoCommand comando,
        CancellationToken cancellationToken)
    {
        var eventoProduto = await _eventoProdutoRepository
            .ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoProdutoNaoEncontrado);

        var evento = await _eventoRepository.ObterPorIdAsync(eventoProduto.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        var produto = await _produtoRepository.ObterPorIdAsync(eventoProduto.ProdutoId, cancellationToken);

        eventoProduto.AjustarEstoque(comando.Quantidade);

        _movimentoEstoqueRepository.Adicionar(MovimentoEstoque.Criar(
            eventoProduto.Id,
            comando.Quantidade,
            MotivoMovimentoEstoque.AjusteManual,
            comando.UsuarioId,
            observacao: comando.Observacao));

        _auditoriaService.Registrar(
            comando.UsuarioId,
            "AjusteEstoque",
            "EventoProduto",
            eventoProduto.Id,
            eventoProduto.EventoId,
            new { comando.Quantidade, comando.Observacao, estoqueAtual = eventoProduto.QuantidadeEstoque });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return eventoProduto.ParaDTO(produto?.Nome ?? string.Empty);
    }
}
