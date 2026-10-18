using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Confirmação da recarga: valor recebido e novo saldo do cartão.</summary>
public partial class RecargaConcluidaViewModel : BaseViewModel
{
    public RecargaConcluidaViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    [ObservableProperty]
    private string cartao = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoValor))]
    private decimal valor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoSaldo))]
    private decimal saldoAtual;

    [ObservableProperty]
    private string numeroRecarga = string.Empty;

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private Guid setorId;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    private string setorNome = string.Empty;

    public string TextoValor => Formatos.Moeda(Valor);

    public string TextoSaldo => Formatos.Moeda(SaldoAtual);

    public string TextoCartao => $"Cartão {Cartao} · recarga nº {NumeroRecarga}";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        SetorId = Parametros.Guid(parametros, "setorId");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
        SetorNome = Parametros.Texto(parametros, "setorNome");

        var recarga = Parametros.Objeto<RecargaResponse>(parametros, "recarga");
        if (recarga is null)
            return;

        Cartao = recarga.CartaoCodigo;
        Valor = recarga.Valor;
        SaldoAtual = recarga.SaldoAtual;
        NumeroRecarga = recarga.RecargaId.ToString()[..8].ToUpperInvariant();
        OnPropertyChanged(nameof(TextoCartao));
    }

    [RelayCommand]
    private Task RecarregarOutroAsync()
        => Navegacao.VoltarParaLeituraAsync(
            "recarga",
            $"../leitura?origem=recarga&eventoId={EventoId}&setorId={SetorId}" +
            $"&eventoNome={Uri.EscapeDataString(EventoNome)}&setorNome={Uri.EscapeDataString(SetorNome)}");

    /// <summary>Volta para o terminal do evento (recarga, venda e saldo).</summary>
    [RelayCommand]
    private Task IrParaOperacaoAsync() => Navegacao.VoltarParaOperacaoAsync();
}
