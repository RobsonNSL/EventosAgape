using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Tela de operação do evento: as três ações do terminal (recarga, venda e saldo)
/// e as últimas operações feitas neste aparelho.
/// </summary>
public partial class OperacaoEventoViewModel : BaseViewModel
{
    private readonly CacheService _cache;

    public OperacaoEventoViewModel(ApiClient api, SessaoService sessao, CacheService cache) : base(api, sessao)
    {
        _cache = cache;
    }

    public ColecaoObservavel<OperacaoRecente> Recentes { get; } = [];

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private Guid setorId;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    private string setorNome = string.Empty;

    [ObservableProperty]
    private bool temRecentes;

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        SetorId = Parametros.Guid(parametros, "setorId");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
        SetorNome = Parametros.Texto(parametros, "setorNome");
    }

    /// <summary>Lê do cache local as últimas operações deste aparelho no evento.</summary>
    public async Task CarregarRecentesAsync()
    {
        if (EventoId == Guid.Empty)
            return;

        var recentes = await _cache.LerRecentesAsync(EventoId);

        Recentes.Substituir(recentes);

        TemRecentes = Recentes.Count > 0;
    }

    [RelayCommand]
    private Task RecarregarAsync() => AbrirLeituraAsync("recarga");

    [RelayCommand]
    private Task VenderAsync() => AbrirLeituraAsync("venda");

    [RelayCommand]
    private Task ConsultarSaldoAsync() => AbrirLeituraAsync("saldo");

    private async Task AbrirLeituraAsync(string origem)
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"leitura?origem={origem}&eventoId={EventoId}&setorId={SetorId}" +
            $"&eventoNome={Uri.EscapeDataString(EventoNome)}&setorNome={Uri.EscapeDataString(SetorNome)}");
    }
}
