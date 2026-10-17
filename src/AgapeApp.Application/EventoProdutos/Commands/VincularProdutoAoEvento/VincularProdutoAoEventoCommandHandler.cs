using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.EventoProdutos.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.EventoProdutos.Commands.VincularProdutoAoEvento;

public class VincularProdutoAoEventoCommandHandler
    : IRequestHandler<VincularProdutoAoEventoCommand, EventoProdutoDTO>
{
    private readonly IEventoRepository _eventoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IEventoProdutoRepository _eventoProdutoRepository;
    private readonly IMovimentoEstoqueRepository _movimentoEstoqueRepository;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public VincularProdutoAoEventoCommandHandler(
        IEventoRepository eventoRepository,
        IProdutoRepository produtoRepository,
        IEventoProdutoRepository eventoProdutoRepository,
        IMovimentoEstoqueRepository movimentoEstoqueRepository,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _eventoRepository = eventoRepository;
        _produtoRepository = produtoRepository;
        _eventoProdutoRepository = eventoProdutoRepository;
        _movimentoEstoqueRepository = movimentoEstoqueRepository;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoProdutoDTO> Handle(
        VincularProdutoAoEventoCommand comando,
        CancellationToken cancellationToken)
    {
        var evento = await _eventoRepository.ObterPorIdAsync(comando.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        var produto = await _produtoRepository.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.ProdutoNaoEncontrado);

        var jaVinculado = await _eventoProdutoRepository
            .ObterPorEventoEProdutoAsync(comando.EventoId, comando.ProdutoId, cancellationToken);

        if (jaVinculado is not null)
            throw new ConflitoException(MensagensErro.ProdutoJaVinculadoAoEvento);

        var eventoProduto = EventoProduto.Criar(
            comando.EventoId,
            comando.ProdutoId,
            comando.Preco,
            comando.ControlaEstoque,
            comando.QuantidadeEstoqueInicial);

        _eventoProdutoRepository.Adicionar(eventoProduto);

        // Estoque inicial registrado como movimentação quando há controle de estoque.
        if (eventoProduto.ControlaEstoque && eventoProduto.QuantidadeEstoque > 0)
        {
            _movimentoEstoqueRepository.Adicionar(MovimentoEstoque.Criar(
                eventoProduto.Id,
                eventoProduto.QuantidadeEstoque,
                MotivoMovimentoEstoque.EstoqueInicial,
                comando.UsuarioId));
        }

        _auditoriaService.Registrar(
            comando.UsuarioId,
            "VincularProdutoAoEvento",
            "EventoProduto",
            eventoProduto.Id,
            comando.EventoId,
            new { comando.ProdutoId, comando.Preco, comando.ControlaEstoque, comando.QuantidadeEstoqueInicial });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return eventoProduto.ParaDTO(produto.Nome);
    }
}
