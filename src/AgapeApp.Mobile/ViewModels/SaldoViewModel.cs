using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Consulta de saldo do cartão no evento, com atalhos para recarga e venda.</summary>
public partial class SaldoViewModel : BaseViewModel
{
    public SaldoViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

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
    [NotifyPropertyChangedFor(nameof(StatusTexto), nameof(StatusFundo), nameof(StatusCor), nameof(MostrarBloqueio), nameof(PodeOperar))]
    private bool cartaoAtivo = true;

    public string TextoSaldo => SaldoAtual is null ? "—" : Formatos.Moeda(SaldoAtual.Value);

    public string TextoCartao => $"Cartão {Cartao}";

    public string StatusTexto => CartaoAtivo ? "Cartão ativo" : "Cartão bloqueado";

    public Color StatusFundo => CartaoAtivo ? Cores.VerdeFundo : Cores.PerigoFundo;

    public Color StatusCor => CartaoAtivo ? Cores.VerdeTexto : Cores.PerigoTexto;

    public bool MostrarBloqueio => !CartaoAtivo;

    public bool PodeOperar => CartaoAtivo;

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        SetorId = Parametros.Guid(parametros, "setorId");
        Cartao = Parametros.Texto(parametros, "cartao");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
        SetorNome = Parametros.Texto(parametros, "setorNome");
        OnPropertyChanged(nameof(TextoCartao));
    }

    private bool _iniciado;

    public async Task IniciarAsync()
    {
        if (_iniciado)
            return;

        _iniciado = true;

        await ExecutarAsync(async () =>
        {
            var saldo = await Api.GetAsync<CartaoSaldoDTO>(
                $"api/cartoes/{Uri.EscapeDataString(Cartao)}/saldo?eventoId={EventoId}");
            SaldoAtual = saldo.SaldoAtual;
            CartaoAtivo = saldo.Ativo;
        });
    }

    [RelayCommand]
    private async Task RecarregarAsync()
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"../recarga?eventoId={EventoId}&setorId={SetorId}&cartao={Uri.EscapeDataString(Cartao)}" +
            $"&eventoNome={Uri.EscapeDataString(EventoNome)}&setorNome={Uri.EscapeDataString(SetorNome)}");
    }

    [RelayCommand]
    private async Task VenderAsync()
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"../venda?eventoId={EventoId}&setorId={SetorId}&cartao={Uri.EscapeDataString(Cartao)}" +
            $"&eventoNome={Uri.EscapeDataString(EventoNome)}&setorNome={Uri.EscapeDataString(SetorNome)}");
    }

    [RelayCommand]
    private async Task LerOutroCartaoAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("..");
    }
}
