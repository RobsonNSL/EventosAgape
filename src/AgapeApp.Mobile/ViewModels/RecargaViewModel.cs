using AgapeApp.Mobile.Config;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views.Janelas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Recarga do cartão: escolha do valor (atalhos ou valor livre), confirmação do valor
/// antes de aplicar e registro da operação com idempotência.
/// </summary>
public partial class RecargaViewModel : BaseViewModel
{
    private static readonly decimal[] ValoresRapidos = [5m, 10m, 20m, 30m, 50m, 100m];

    private readonly CacheService _cache;

    private Guid? _idOperacao;
    private decimal? _valorDaOperacao;
    private bool _iniciado;
    private bool _aplicandoChip;

    public RecargaViewModel(ApiClient api, SessaoService sessao, CacheService cache) : base(api, sessao)
    {
        _cache = cache;
        Valores.Substituir(ValoresRapidos.Select(ValorChip.De));
    }

    public ColecaoObservavel<ValorChip> Valores { get; } = [];

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
    [NotifyPropertyChangedFor(nameof(TextoSaldo), nameof(TextoNovoSaldo))]
    private decimal? saldoAtual;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBotao), nameof(PodeConfirmar), nameof(TextoNovoSaldo), nameof(MostrarNovoSaldo))]
    private decimal? valorSelecionado;

    /// <summary>Campo "outro valor" (digitado direto na tela, sem janela extra).</summary>
    [ObservableProperty]
    private string valorDigitado = string.Empty;

    public string TextoSaldo => SaldoAtual is null ? "—" : Formatos.Moeda(SaldoAtual.Value);

    public bool MostrarNovoSaldo => ValorSelecionado is > 0 && SaldoAtual is not null;

    public string TextoNovoSaldo => SaldoAtual is decimal saldo && ValorSelecionado is decimal valor
        ? $"Saldo após a recarga: {Formatos.Moeda(saldo + valor)}"
        : string.Empty;

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

    /// <summary>Busca o saldo uma única vez (fechar a janela de confirmação não refaz a consulta).</summary>
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

            if (!saldo.Ativo)
                MensagemErro = "Este cartão está bloqueado. Procure o administrador.";
        });
    }

    [RelayCommand]
    private void SelecionarValor(ValorChip? chip)
    {
        if (chip is null)
            return;

        foreach (var valor in Valores)
            valor.Selecionado = ReferenceEquals(valor, chip);

        _aplicandoChip = true;
        ValorDigitado = string.Empty;
        _aplicandoChip = false;

        MensagemErro = null;
        ValorSelecionado = chip.Valor;
    }

    partial void OnValorDigitadoChanged(string value)
    {
        if (_aplicandoChip)
            return;

        foreach (var chip in Valores)
            chip.Selecionado = false;

        ValorSelecionado = Formatos.TentarValor(value, out var valor) && valor > 0 ? valor : null;
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        if (ValorSelecionado is not decimal valor || valor <= 0)
        {
            MensagemErro = string.IsNullOrWhiteSpace(ValorDigitado)
                ? "Escolha o valor da recarga."
                : "Valor inválido. Digite, por exemplo, 15,50.";
            return;
        }

        // Regra do evento: o valor aparece antes de ser aplicado e o operador confirma.
        var confirmado = await ConfirmarAsync(
            $"Recarregar {Formatos.Moeda(valor)}?",
            $"Cartão {Cartao}. Confirme que o pagamento foi aprovado na maquininha antes de continuar.",
            "Confirmar recarga",
            TomDialogo.Destaque,
            "\uf0d6",
            SaldoAtual is decimal saldo
                ? new List<ItemChecagem>
                {
                    new($"Saldo atual: {Formatos.Moeda(saldo)}", true),
                    new($"Saldo após a recarga: {Formatos.Moeda(saldo + valor)}", true)
                }
                : null);

        if (!confirmado)
            return;

        await ExecutarAsync(async () =>
        {
            // Retry (sem conexão, toque duplo) reusa o mesmo id: a API não duplica a recarga.
            if (_idOperacao is null || _valorDaOperacao != valor)
            {
                _idOperacao = Guid.NewGuid();
                _valorDaOperacao = valor;
            }

            var resposta = await Api.PostIdempotenteAsync<RecargaResponse>("api/recargas", new RecargaRequest(
                Cartao,
                EventoId,
                SetorId,
                valor,
                _idOperacao.Value,
                AppConfig.Dispositivo));

            _idOperacao = null;
            _valorDaOperacao = null;

            await _cache.RegistrarRecargaAsync(resposta, SetorNome);
            Vibrar();

            // "../" troca esta tela pela confirmação: a pilha não cresce a cada cliente.
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("../recarga-concluida", new ShellNavigationQueryParameters
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

    private static void Vibrar()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        }
        catch (Exception)
        {
            // Aparelho sem vibração.
        }
    }
}
