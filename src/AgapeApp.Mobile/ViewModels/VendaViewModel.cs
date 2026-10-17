using System.Collections.ObjectModel;
using System.Net;
using AgapeApp.Mobile.Config;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Venda no setor: produtos do catálogo do setor com quantidade e total.</summary>
public partial class VendaViewModel : BaseViewModel
{
    private readonly CacheService _cache;

    private Guid? _idOperacao;

    public VendaViewModel(ApiClient api, SessaoService sessao, CacheService cache) : base(api, sessao)
    {
        _cache = cache;
    }

    public ObservableCollection<ItemCarrinho> Produtos { get; } = [];

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

    public decimal Total => Produtos.Sum(p => p.Subtotal);

    public string TextoTotal => Formatos.Moeda(Total);

    public bool TemItens => Produtos.Any(p => p.Quantidade > 0);

    public string TextoBotao => TemItens ? $"Confirmar venda · {TextoTotal}" : "Adicione itens à venda";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        SetorId = Parametros.Guid(parametros, "setorId");
        Cartao = Parametros.Texto(parametros, "cartao");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
        SetorNome = Parametros.Texto(parametros, "setorNome");
    }

    public async Task IniciarAsync()
    {
        await ExecutarAsync(async () =>
        {
            var semRede = false;

            try
            {
                var saldo = await Api.GetAsync<CartaoSaldoDTO>(
                    $"api/cartoes/{Uri.EscapeDataString(Cartao)}/saldo?eventoId={EventoId}");
                SaldoAtual = saldo.SaldoAtual;
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                semRede = true;
                SaldoAtual = null;
            }

            List<EventoProdutoDTO> produtos;
            try
            {
                produtos = await Api.GetAsync<List<EventoProdutoDTO>>($"api/setores/{SetorId}/produtos");
                await _cache.SalvarProdutosDoSetorAsync(SetorId, produtos);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                var salvos = await _cache.LerProdutosDoSetorAsync(SetorId);
                if (salvos.Count == 0)
                    throw;

                produtos = salvos.ToList();
                semRede = true;
            }

            ModoOffline = semRede;

            Produtos.Clear();
            foreach (var dto in produtos)
            {
                var item = ItemCarrinho.De(dto);
                item.PropertyChanged += (_, _) =>
                {
                    // Carrinho mudou: a próxima confirmação é uma nova operação.
                    _idOperacao = null;
                    NotificarTotais();
                };
                Produtos.Add(item);
            }

            NotificarTotais();
        });
    }

    [RelayCommand]
    private void Aumentar(ItemCarrinho? item)
    {
        if (item is null)
            return;

        if (item.ControlaEstoque && item.Quantidade >= item.Estoque)
        {
            MensagemErro = $"Sem estoque suficiente de {item.Nome}.";
            return;
        }

        item.Quantidade++;
    }

    [RelayCommand]
    private void Diminuir(ItemCarrinho? item)
    {
        if (item is not null && item.Quantidade > 0)
            item.Quantidade--;
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

        await ExecutarAsync(async () =>
        {
            // Retry reusa o mesmo id: a API não debita duas vezes o mesmo pedido.
            _idOperacao ??= Guid.NewGuid();

            var resposta = await Api.PostAsync<VendaResponse>("api/vendas", new VendaRequest(
                Cartao,
                EventoId,
                SetorId,
                itens.Select(i => new ItemVendaRequest(i.EventoProdutoId, i.Quantidade)).ToList(),
                _idOperacao.Value,
                AppConfig.Dispositivo));

            _idOperacao = null;

            await _cache.RegistrarVendaAsync(resposta, SetorNome);

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("venda-concluida", new ShellNavigationQueryParameters
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

    private void NotificarTotais()
    {
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(TextoTotal));
        OnPropertyChanged(nameof(TemItens));
        OnPropertyChanged(nameof(TextoBotao));
    }
}
