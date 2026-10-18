using System.ComponentModel;
using AgapeApp.Mobile.Config;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Venda no setor: grade de produtos do setor (toque no card soma 1), total e saldo
/// que sobra no cartão — o botão já avisa quando o saldo não cobre o pedido.
/// </summary>
public partial class VendaViewModel : BaseViewModel
{
    private readonly CacheService _cache;

    private Guid? _idOperacao;
    private bool _iniciado;

    public VendaViewModel(ApiClient api, SessaoService sessao, CacheService cache) : base(api, sessao)
    {
        _cache = cache;
    }

    public ColecaoObservavel<ItemCarrinho> Produtos { get; } = [];

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private Guid setorId;

    [ObservableProperty]
    private string cartao = string.Empty;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    private string setorNome = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoSaldo))]
    private decimal? saldoAtual;

    [ObservableProperty]
    private bool modoOffline;

    public string TextoSaldo => SaldoAtual is null ? "—" : Formatos.Moeda(SaldoAtual.Value);

    public decimal Total { get; private set; }

    public int QuantidadeItens { get; private set; }

    public string TextoTotal => Formatos.Moeda(Total);

    public bool TemItens => QuantidadeItens > 0;

    /// <summary>Com o saldo conhecido, a venda que passa do saldo nem é enviada.</summary>
    public bool SaldoInsuficiente => SaldoAtual is decimal saldo && Total > saldo;

    public bool PodeConfirmar => TemItens && !SaldoInsuficiente;

    public string TextoItens => QuantidadeItens switch
    {
        0 => "Nenhum item",
        1 => "1 item",
        _ => $"{QuantidadeItens} itens"
    };

    public string TextoRestante => SaldoAtual is decimal saldo
        ? SaldoInsuficiente
            ? $"Faltam {Formatos.Moeda(Total - saldo)} no cartão"
            : $"Sobra {Formatos.Moeda(saldo - Total)} no cartão"
        : "Saldo será conferido na confirmação";

    public string TextoBotao => !TemItens
        ? "Toque nos produtos para vender"
        : SaldoInsuficiente
            ? "Saldo insuficiente"
            : $"Confirmar venda · {TextoTotal}";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        SetorId = Parametros.Guid(parametros, "setorId");
        Cartao = Parametros.Texto(parametros, "cartao");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
        SetorNome = Parametros.Texto(parametros, "setorNome");
    }

    /// <summary>Carrega saldo e cardápio uma vez: voltar a esta tela não zera o carrinho.</summary>
    public async Task IniciarAsync()
    {
        if (_iniciado)
            return;

        _iniciado = true;

        await ExecutarAsync(async () =>
        {
            var saldoTask = BuscarSaldoAsync();
            var produtosTask = BuscarProdutosAsync();

            await Task.WhenAll(saldoTask, produtosTask);

            var (produtos, cardapioSalvo) = produtosTask.Result;
            ModoOffline = cardapioSalvo || !saldoTask.Result;

            foreach (var antigo in Produtos)
                antigo.PropertyChanged -= AoMudarItem;

            var itens = produtos.Select(ItemCarrinho.De).ToList();
            foreach (var item in itens)
                item.PropertyChanged += AoMudarItem;

            Produtos.Substituir(itens);
            RecalcularTotais();
        });
    }

    /// <summary>Saldo do cartão; devolve false quando não deu para consultar (sem rede).</summary>
    private async Task<bool> BuscarSaldoAsync()
    {
        try
        {
            var saldo = await Api.GetAsync<CartaoSaldoDTO>(
                $"api/cartoes/{Uri.EscapeDataString(Cartao)}/saldo?eventoId={EventoId}");
            SaldoAtual = saldo.SaldoAtual;
            return true;
        }
        catch (ApiException ex) when (ex.SemConexao)
        {
            SaldoAtual = null;
            return false;
        }
    }

    /// <summary>Cardápio do setor; sem rede, usa o salvo neste aparelho.</summary>
    private async Task<(IReadOnlyList<EventoProdutoDTO> Produtos, bool DoCache)> BuscarProdutosAsync()
    {
        try
        {
            var produtos = await Api.GetAsync<List<EventoProdutoDTO>>($"api/setores/{SetorId}/produtos");
            await _cache.SalvarProdutosDoSetorAsync(SetorId, produtos);
            return (produtos, false);
        }
        catch (ApiException ex) when (ex.SemConexao)
        {
            var salvos = await _cache.LerProdutosDoSetorAsync(SetorId);
            if (salvos.Count == 0)
                throw;

            return (salvos, true);
        }
    }

    /// <summary>Toque no card do produto: soma uma unidade (grade de toque rápido).</summary>
    [RelayCommand]
    private void Adicionar(ItemCarrinho? item) => Aumentar(item);

    [RelayCommand]
    private void Aumentar(ItemCarrinho? item)
    {
        if (item is null)
            return;

        if (item.ControlaEstoque && item.Quantidade >= item.Estoque)
        {
            MensagemErro = item.Estoque <= 0
                ? $"{item.Nome} está sem estoque."
                : $"Só há {item.Estoque} de {item.Nome} em estoque.";
            return;
        }

        MensagemErro = null;
        item.Quantidade++;
    }

    [RelayCommand]
    private void Diminuir(ItemCarrinho? item)
    {
        if (item is not null && item.Quantidade > 0)
            item.Quantidade--;
    }

    [RelayCommand]
    private void Limpar()
    {
        foreach (var item in Produtos)
            item.Quantidade = 0;

        MensagemErro = null;
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        var itens = Produtos.Where(p => p.Quantidade > 0).ToList();
        if (itens.Count == 0)
        {
            MensagemErro = "Adicione pelo menos um item à venda.";
            return;
        }

        if (SaldoInsuficiente)
        {
            MensagemErro = $"Saldo insuficiente: o cartão tem {TextoSaldo} e a venda é de {TextoTotal}.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            // Retry reusa o mesmo id: a API não debita duas vezes o mesmo pedido.
            _idOperacao ??= Guid.NewGuid();

            var resposta = await Api.PostIdempotenteAsync<VendaResponse>("api/vendas", new VendaRequest(
                Cartao,
                EventoId,
                SetorId,
                itens.Select(i => new ItemVendaRequest(i.EventoProdutoId, i.Quantidade)).ToList(),
                _idOperacao.Value,
                AppConfig.Dispositivo));

            _idOperacao = null;

            await _cache.RegistrarVendaAsync(resposta, SetorNome);

            try
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            }
            catch (Exception)
            {
                // Aparelho sem vibração.
            }

            // "../" troca esta tela pela confirmação: a pilha não cresce a cada cliente.
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("../venda-concluida", new ShellNavigationQueryParameters
                {
                    ["venda"] = resposta,
                    ["eventoId"] = EventoId,
                    ["setorId"] = SetorId,
                    ["eventoNome"] = EventoNome,
                    ["setorNome"] = SetorNome
                });
            }
        });
    }

    protected override string MensagemDeErro(ApiException ex)
    {
        // A API devolve o saldo atual quando recusa por saldo: a tela já mostra o valor certo.
        if (ex.SaldoAtual is decimal saldo)
        {
            SaldoAtual = saldo;
            RecalcularTotais();
        }

        return base.MensagemDeErro(ex);
    }

    private void AoMudarItem(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ItemCarrinho.Quantidade))
            return;

        // Carrinho mudou: a próxima confirmação é uma nova operação.
        _idOperacao = null;
        RecalcularTotais();
    }

    private void RecalcularTotais()
    {
        Total = Produtos.Sum(p => p.Subtotal);
        QuantidadeItens = Produtos.Sum(p => p.Quantidade);

        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(QuantidadeItens));
        OnPropertyChanged(nameof(TextoTotal));
        OnPropertyChanged(nameof(TemItens));
        OnPropertyChanged(nameof(TextoItens));
        OnPropertyChanged(nameof(SaldoInsuficiente));
        OnPropertyChanged(nameof(PodeConfirmar));
        OnPropertyChanged(nameof(TextoRestante));
        OnPropertyChanged(nameof(TextoBotao));
    }
}
