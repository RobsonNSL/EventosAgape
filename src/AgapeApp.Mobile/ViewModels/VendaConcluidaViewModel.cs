using System.Collections.ObjectModel;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Confirmação da venda: itens, total e saldo restante do cartão.</summary>
public partial class VendaConcluidaViewModel : BaseViewModel
{
    public VendaConcluidaViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ObservableCollection<ItemVendaCartao> Itens { get; } = [];

    [ObservableProperty]
    private string cartao = string.Empty;

    [ObservableProperty]
    private string numeroVenda = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoTotal))]
    private decimal total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoSaldo))]
    private decimal saldoAtual;

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private Guid setorId;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    private string setorNome = string.Empty;

    public string TextoTotal => Formatos.Moeda(Total);

    public string TextoSaldo => Formatos.Moeda(SaldoAtual);

    public string TextoCartao => $"Cartão {Cartao} · venda nº {NumeroVenda}";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        SetorId = Parametros.Guid(parametros, "setorId");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
        SetorNome = Parametros.Texto(parametros, "setorNome");

        var venda = Parametros.Objeto<VendaResponse>(parametros, "venda");
        if (venda is null)
            return;

        Cartao = venda.CartaoCodigo;
        Total = venda.Total;
        SaldoAtual = venda.SaldoAtual;
        NumeroVenda = venda.VendaId.ToString()[..8].ToUpperInvariant();

        Itens.Clear();
        foreach (var item in venda.Itens)
        {
            Itens.Add(new ItemVendaCartao
            {
                Nome = item.ProdutoNome,
                Quantidade = item.Quantidade,
                Subtotal = item.Total
            });
        }

        OnPropertyChanged(nameof(TextoCartao));
    }

    [RelayCommand]
    private Task ProximoClienteAsync()
        => Navegacao.VoltarParaLeituraAsync(
            "venda",
            $"../leitura?origem=venda&eventoId={EventoId}&setorId={SetorId}" +
            $"&eventoNome={Uri.EscapeDataString(EventoNome)}&setorNome={Uri.EscapeDataString(SetorNome)}");

    /// <summary>Volta para o terminal do evento (recarga, venda e saldo).</summary>
    [RelayCommand]
    private Task IrParaOperacaoAsync() => Navegacao.VoltarParaOperacaoAsync();
}
