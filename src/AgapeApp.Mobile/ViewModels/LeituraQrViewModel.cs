using System.Net;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Leitura do cartão: o operador escolhe ler o QR (câmera) ou digitar o código. Antes de
/// seguir para recarga/venda/saldo o app confere na API se o cartão existe — leitura
/// errada ou código digitado errado ficam nesta tela, com o aviso, sem abrir nada.
/// </summary>
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

    /// <summary>"escolha" (duas opções), "camera" ou "manual".</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarEscolha), nameof(MostrarCamera), nameof(MostrarManual))]
    private string modo = "escolha";

    public bool MostrarEscolha => Modo == "escolha";

    public bool MostrarCamera => Modo == "camera";

    public bool MostrarManual => Modo == "manual";

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
    private void UsarCamera()
    {
        MensagemErro = null;
        Modo = "camera";
    }

    [RelayCommand]
    private void UsarDigitacao()
    {
        MensagemErro = null;
        Modo = "manual";
    }

    [RelayCommand]
    private Task ConfirmarAsync() => ConfirmarCodigoAsync(Codigo);

    /// <summary>
    /// Confere o cartão na API e, se existir, abre a operação. Devolve false quando o cartão
    /// não serve (não cadastrado, bloqueado, sem rede) — a câmera continua lendo.
    /// </summary>
    public async Task<bool> ConfirmarCodigoAsync(string? digitado)
    {
        var limpo = CodigoCartao.Normalizar(digitado);
        if (string.IsNullOrWhiteSpace(limpo))
        {
            MensagemErro = "Digite o código impresso no cartão.";
            return false;
        }

        var cartao = await ExecutarAsync(() => Api.GetAsync<CartaoSaldoDTO>(
            $"api/cartoes/{Uri.EscapeDataString(limpo)}/saldo?eventoId={EventoId}"));

        if (cartao is null)
            return false;

        if (!cartao.Ativo)
        {
            MensagemErro = $"O cartão {limpo} está bloqueado. Procure o administrador.";
            return false;
        }

        Codigo = limpo;

        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync(
                $"{Origem}?eventoId={EventoId}&setorId={SetorId}&cartao={Uri.EscapeDataString(limpo)}" +
                $"&eventoNome={Uri.EscapeDataString(EventoNome)}&setorNome={Uri.EscapeDataString(SetorNome)}");
        }

        return true;
    }

    protected override string MensagemDeErro(ApiException ex)
        => ex.StatusCode == HttpStatusCode.NotFound
            ? "Cartão não encontrado. Confira o código ou peça ao administrador para cadastrá-lo."
            : base.MensagemDeErro(ex);
}
