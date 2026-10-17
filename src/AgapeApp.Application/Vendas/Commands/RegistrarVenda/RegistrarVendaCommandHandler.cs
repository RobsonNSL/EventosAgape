using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Operacoes;
using AgapeApp.Application.Vendas.DTOs;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Vendas.Commands.RegistrarVenda;

public class RegistrarVendaCommandHandler : IRequestHandler<RegistrarVendaCommand, VendaResponse>
{
    private readonly IVendaRepository _vendaRepository;
    private readonly IEventoProdutoRepository _eventoProdutoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly ISetorProdutoRepository _setorProdutoRepository;
    private readonly IMovimentoEstoqueRepository _movimentoEstoqueRepository;
    private readonly ISaldoCartaoEventoRepository _saldoCartaoEventoRepository;
    private readonly ICartaoRepository _cartaoRepository;
    private readonly OperacaoContextoService _operacaoContextoService;
    private readonly AuditoriaService _auditoriaService;
    private readonly INotificadorDashboard _notificadorDashboard;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarVendaCommandHandler(
        IVendaRepository vendaRepository,
        IEventoProdutoRepository eventoProdutoRepository,
        IProdutoRepository produtoRepository,
        ISetorProdutoRepository setorProdutoRepository,
        IMovimentoEstoqueRepository movimentoEstoqueRepository,
        ISaldoCartaoEventoRepository saldoCartaoEventoRepository,
        ICartaoRepository cartaoRepository,
        OperacaoContextoService operacaoContextoService,
        AuditoriaService auditoriaService,
        INotificadorDashboard notificadorDashboard,
        IUnitOfWork unitOfWork)
    {
        _vendaRepository = vendaRepository;
        _eventoProdutoRepository = eventoProdutoRepository;
        _produtoRepository = produtoRepository;
        _setorProdutoRepository = setorProdutoRepository;
        _movimentoEstoqueRepository = movimentoEstoqueRepository;
        _saldoCartaoEventoRepository = saldoCartaoEventoRepository;
        _cartaoRepository = cartaoRepository;
        _operacaoContextoService = operacaoContextoService;
        _auditoriaService = auditoriaService;
        _notificadorDashboard = notificadorDashboard;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendaResponse> Handle(
        RegistrarVendaCommand comando,
        CancellationToken cancellationToken)
    {
        // Idempotência: retry ou toque duplo do app devolve a venda já registrada.
        var vendaExistente = await _vendaRepository
            .ObterPorIdOperacaoAsync(comando.IdOperacaoCliente, cancellationToken);

        if (vendaExistente is not null)
            return await MontarRespostaAsync(vendaExistente, cancellationToken);

        // Validações comuns às operações (cartão ativo, evento aberto, setor, perfil e designação).
        var contexto = await _operacaoContextoService.ValidarAsync(
            comando.CartaoCodigo,
            comando.EventoId,
            comando.SetorId,
            comando.UsuarioId,
            cancellationToken);

        // Itens repetidos do mesmo produto viram uma única linha (quantidades somadas).
        var itensAgrupados = comando.Itens
            .GroupBy(item => item.EventoProdutoId)
            .Select(grupo => new ItemVendaPedido(grupo.Key, grupo.Sum(item => item.Quantidade)))
            .ToList();

        // Vínculos rastreados: validados e, quando o estoque é controlado, baixados na sequência.
        var eventoProdutos = await _eventoProdutoRepository
            .ObterParaAtualizarPorIdsAsync(
                itensAgrupados.Select(item => item.EventoProdutoId),
                cancellationToken);

        if (eventoProdutos.Count != itensAgrupados.Count)
            throw new RegraNegocioException(MensagensErro.EventoProdutoNaoEncontrado);

        var produtos = await _produtoRepository
            .ObterPorIdsAsync(eventoProdutos.Select(eventoProduto => eventoProduto.ProdutoId), cancellationToken);

        var nomesPorProdutoId = produtos.ToDictionary(produto => produto.Id, produto => produto.Nome);

        // O setor só vende os produtos vinculados a ele (setor_produtos).
        var vinculosDoSetor = await _setorProdutoRepository
            .ListarPorSetorAsync(contexto.Setor.Id, cancellationToken);

        var produtosPermitidos = vinculosDoSetor.Select(vinculo => vinculo.EventoProdutoId).ToHashSet();

        var eventoProdutosPorId = eventoProdutos.ToDictionary(eventoProduto => eventoProduto.Id);

        // Primeira passada: valida cada item e soma o total (nada é alterado antes de tudo válido).
        var linhas = new List<(EventoProduto EventoProduto, string Nome, int Quantidade)>();
        var total = 0m;

        foreach (var itemAgrupado in itensAgrupados)
        {
            var eventoProduto = eventoProdutosPorId[itemAgrupado.EventoProdutoId];
            var nome = nomesPorProdutoId.GetValueOrDefault(eventoProduto.ProdutoId, string.Empty);

            if (eventoProduto.EventoId != contexto.Evento.Id)
                throw new RegraNegocioException(MensagensErro.ProdutoDeOutroEvento);

            if (!produtosPermitidos.Contains(eventoProduto.Id))
                throw new RegraNegocioException(string.Format(MensagensErro.ProdutoForaDoSetor, nome));

            if (eventoProduto.ControlaEstoque && eventoProduto.QuantidadeEstoque < itemAgrupado.Quantidade)
                throw new RegraNegocioException(string.Format(MensagensErro.EstoqueInsuficiente, nome));

            total += eventoProduto.Preco * itemAgrupado.Quantidade;
            linhas.Add((eventoProduto, nome, itemAgrupado.Quantidade));
        }

        // Saldo: carrega ou cria; Debitar lança SaldoInsuficienteException (400 com o saldo atual).
        var saldo = await _saldoCartaoEventoRepository
            .ObterAsync(contexto.Cartao.Id, contexto.Evento.Id, cancellationToken);

        if (saldo is null)
        {
            saldo = SaldoCartaoEvento.Criar(contexto.Cartao.Id, contexto.Evento.Id);
            _saldoCartaoEventoRepository.Adicionar(saldo);
        }

        saldo.Debitar(total);

        var venda = Venda.Criar(
            contexto.Evento.Id,
            contexto.Cartao.Id,
            contexto.Setor.Id,
            contexto.Usuario.Id,
            total,
            comando.IdOperacaoCliente,
            comando.Dispositivo);

        _vendaRepository.Adicionar(venda);

        // Segunda passada: grava os itens (retrato do preço) e baixa o estoque controlado.
        var itensResposta = new List<ItemVendaDTO>();

        foreach (var linha in linhas)
        {
            var item = ItemVenda.Criar(
                venda.Id,
                linha.EventoProduto.Id,
                linha.Nome,
                linha.EventoProduto.Preco,
                linha.Quantidade);

            _vendaRepository.AdicionarItem(item);

            if (linha.EventoProduto.ControlaEstoque)
            {
                linha.EventoProduto.RetirarEstoque(linha.Quantidade);

                _movimentoEstoqueRepository.Adicionar(MovimentoEstoque.Criar(
                    linha.EventoProduto.Id,
                    -linha.Quantidade,
                    MotivoMovimentoEstoque.Venda,
                    contexto.Usuario.Id,
                    vendaId: venda.Id));
            }

            itensResposta.Add(new ItemVendaDTO(
                item.EventoProdutoId,
                item.ProdutoNome,
                item.PrecoUnitario,
                item.Quantidade,
                item.Total));
        }

        _auditoriaService.Registrar(
            contexto.Usuario.Id,
            "Venda",
            "Venda",
            venda.Id,
            contexto.Evento.Id,
            new
            {
                total,
                cartao = contexto.Cartao.Codigo,
                setor = contexto.Setor.Nome,
                itens = linhas.Select(linha => new { produto = linha.Nome, linha.Quantidade })
            },
            comando.Dispositivo);

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        var resposta = new VendaResponse(
            venda.Id,
            contexto.Cartao.Codigo,
            contexto.Evento.Id,
            venda.Total,
            saldo.Saldo,
            itensResposta,
            venda.CriadoEm);

        await _notificadorDashboard.NotificarAsync(
            contexto.Evento.Id,
            "Venda",
            resposta,
            cancellationToken);

        return resposta;
    }

    private async Task<VendaResponse> MontarRespostaAsync(
        Venda venda,
        CancellationToken cancellationToken)
    {
        var cartao = await _cartaoRepository.ObterPorIdAsync(venda.CartaoId, cancellationToken);

        var saldo = await _saldoCartaoEventoRepository
            .ObterAsync(venda.CartaoId, venda.EventoId, cancellationToken);

        var itens = await _vendaRepository.ListarItensPorVendaAsync(venda.Id, cancellationToken);

        var itensResposta = itens
            .Select(item => new ItemVendaDTO(
                item.EventoProdutoId,
                item.ProdutoNome,
                item.PrecoUnitario,
                item.Quantidade,
                item.Total))
            .ToList();

        return new VendaResponse(
            venda.Id,
            cartao?.Codigo ?? string.Empty,
            venda.EventoId,
            venda.Total,
            saldo?.Saldo ?? 0m,
            itensResposta,
            venda.CriadoEm);
    }
}
