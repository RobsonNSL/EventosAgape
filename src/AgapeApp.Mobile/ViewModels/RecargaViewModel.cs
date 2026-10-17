using System.Collections.ObjectModel;
using AgapeApp.Mobile.Config;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Recarga do cartão: escolha do valor e registro da operação (com idempotência).</summary>
public partial class RecargaViewModel : BaseViewModel
{
    private static readonly decimal[] ValoresRapidos = [5m, 10m, 20m, 50m, 100m];

    private readonly CacheService _cache;

    private Guid? _idOperacao;
    private decimal? _valorDaOperacao;

    public RecargaViewModel(ApiClient api, SessaoService sessao, CacheService cache) : base(api, sessao)
    {
        _cache = cache;

        foreach (var valor in ValoresRapidos)
            Valores.Add(ValorChip.De(valor));
    }

    public ObservableCollection<ValorChip> Valores { get; } = [];

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
    [NotifyPropertyChangedFor(nameof(TextoBotao))]
    [NotifyPropertyChangedFor(nameof(PodeConfirmar))]
    private decimal? valorSelecionado;

    public string TextoSaldo => SaldoAtual is null ? "—" : Formatos.Moeda(SaldoAtual.Value);

    public string TextoBotao => ValorSelecionado is > 0
        ? $"Recarregar {Formatos.Moeda(ValorSelecionado.Value)}"
        : "Escolha um valor";

    public bool PodeConfirmar => ValorSelecionado is > 0;

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
            var saldo = await Api.GetAsync<CartaoSaldoDTO>(
                $"api/cartoes/{Uri.EscapeDataString(Cartao)}/saldo?eventoId={EventoId}");
            SaldoAtual = saldo.SaldoAtual;
        });
    }

    [RelayCommand]
    private void SelecionarValor(ValorChip? chip)
    {
        if (chip is null)
            return;

        foreach (var valor in Valores)
            valor.Selecionado = ReferenceEquals(valor, chip);

        ValorSelecionado = chip.Valor;
    }

    /// <summary>Valor digitado manualmente ("Outro valor").</summary>
    public void DefinirValorPersonalizado(string? texto)
    {
        if (!Formatos.TentarValor(texto, out var valor) || valor <= 0)
        {
            MensagemErro = "Valor inválido. Digite, por exemplo, 15,50.";
            return;
        }

        foreach (var chip in Valores)
            chip.Selecionado = false;

        ValorSelecionado = valor;
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        if (ValorSelecionado is not > 0)
        {
            MensagemErro = "Escolha o valor da recarga.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            // Retry (sem conexão, toque duplo) reusa o mesmo id: a API não duplica a recarga.
            if (_idOperacao is null || _valorDaOperacao != ValorSelecionado)
            {
                _idOperacao = Guid.NewGuid();
                _valorDaOperacao = ValorSelecionado;
            }

            var resposta = await Api.PostAsync<RecargaResponse>("api/recargas", new RecargaRequest(
                Cartao,
                EventoId,
                SetorId,
                ValorSelecionado!.Value,
                _idOperacao.Value,
                AppConfig.Dispositivo));

            _idOperacao = null;
            _valorDaOperacao = null;

            await _cache.RegistrarRecargaAsync(resposta, SetorNome);

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("recarga-concluida", new ShellNavigationQueryParameters
                {
                    ["recarga"] = resposta,
                    ["eventoId"] = EventoId,
                    ["setorId"] = SetorId,
                    ["eventoNome"] = EventoNome,
                    ["setorNome"] = SetorNome
                });
            }
        });
    }
}
