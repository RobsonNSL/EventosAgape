using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Leitura do cartão pela câmera (QR) ou pela digitação do código impresso.</summary>
public partial class LeituraQrViewModel : BaseViewModel
{
    public LeituraQrViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private Guid setorId;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    private string setorNome = string.Empty;

    [ObservableProperty]
    private string origem = "recarga";

    [ObservableProperty]
    private string codigo = string.Empty;

    public string TituloAcao => Origem switch
    {
        "venda" => "Vender no setor",
        "saldo" => "Consultar saldo",
        _ => "Recarregar cartão"
    };

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        SetorId = Parametros.Guid(parametros, "setorId");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
        SetorNome = Parametros.Texto(parametros, "setorNome");
        Origem = Parametros.Texto(parametros, "origem") is { Length: > 0 } origem ? origem : "recarga";
    }

    partial void OnOrigemChanged(string value) => OnPropertyChanged(nameof(TituloAcao));

    /// <summary>Prepara a tela para o próximo cartão (ao voltar de uma venda/recarga concluída).</summary>
    public void PrepararNovaLeitura(string origem)
    {
        Origem = origem;
        Codigo = string.Empty;
        MensagemErro = null;
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        var limpo = (Codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(limpo))
        {
            MensagemErro = "Digite o código impresso no cartão.";
            return;
        }

        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"{Origem}?eventoId={EventoId}&setorId={SetorId}&cartao={Uri.EscapeDataString(limpo)}" +
            $"&eventoNome={Uri.EscapeDataString(EventoNome)}&setorNome={Uri.EscapeDataString(SetorNome)}");
    }
}
