using System.Collections.ObjectModel;
using System.Net;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Dashboard do evento (administrador): totais de recargas e vendas, saldo em
/// circulação, consolidados e atualização ao vivo pelo SignalR.
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly DashboardService _tempoReal;

    private bool _atualizacaoPendente;

    public DashboardViewModel(ApiClient api, SessaoService sessao, DashboardService tempoReal) : base(api, sessao)
    {
        _tempoReal = tempoReal;
    }

    public ObservableCollection<ResumoPorSetorDTO> PorSetor { get; } = [];

    public ObservableCollection<ResumoPorOperadorDTO> PorOperador { get; } = [];

    public ObservableCollection<ResumoPorProdutoDTO> PorProduto { get; } = [];

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    private string situacao = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoRecargas), nameof(TextoQuantidadeRecargas))]
    private int quantidadeRecargas;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoRecargas))]
    private decimal totalRecargas;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoQuantidadeVendas))]
    private int quantidadeVendas;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoVendas))]
    private decimal totalVendas;

    [ObservableProperty]
    private decimal saldoEmCirculacao;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoTempoReal), nameof(CorTempoReal), nameof(FundoTempoReal))]
    private bool aoVivo;

    [ObservableProperty]
    private string textoAtualizacao = "Carregando indicadores...";

    [ObservableProperty]
    private bool temPorSetor;

    [ObservableProperty]
    private bool temPorOperador;

    [ObservableProperty]
    private bool temPorProduto;

    public string TextoRecargas => Formatos.Moeda(TotalRecargas);

    public string TextoQuantidadeRecargas => QuantidadeRecargas == 1 ? "1 recarga" : $"{QuantidadeRecargas} recargas";

    public string TextoVendas => Formatos.Moeda(TotalVendas);

    public string TextoQuantidadeVendas => QuantidadeVendas == 1 ? "1 venda" : $"{QuantidadeVendas} vendas";

    public string TextoSaldoEmCirculacao => Formatos.Moeda(SaldoEmCirculacao);

    public string TextoTempoReal => AoVivo ? "Ao vivo" : "Tempo real indisponível";

    public Color CorTempoReal => AoVivo ? Cores.VerdeTexto : Cores.CinzaTexto;

    public Color FundoTempoReal => AoVivo ? Cores.VerdeFundo : Cores.CinzaFundo;

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
    }

    public async Task IniciarAsync()
    {
        _tempoReal.OperacaoRecebida -= AoReceberOperacao;
        _tempoReal.OperacaoRecebida += AoReceberOperacao;

        await CarregarResumoAsync(mostrarCarregamento: true);

        try
        {
            await _tempoReal.EntrarNoEventoAsync(EventoId);
            AoVivo = true;
        }
        catch (Exception)
        {
            AoVivo = false;
        }
    }

    /// <summary>Encerra o acompanhamento ao sair da tela.</summary>
    public async Task PararAsync()
    {
        _tempoReal.OperacaoRecebida -= AoReceberOperacao;
        await _tempoReal.SairDoEventoAsync();
        AoVivo = false;
    }

    [RelayCommand]
    private async Task AtualizarAsync() => await CarregarResumoAsync(mostrarCarregamento: true);

    [RelayCommand]
    private async Task AbrirSaldosAsync()
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"saldos?eventoId={EventoId}&eventoNome={Uri.EscapeDataString(EventoNome)}");
    }

    [RelayCommand]
    private async Task AbrirAuditoriaAsync()
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"auditoria?eventoId={EventoId}&eventoNome={Uri.EscapeDataString(EventoNome)}");
    }

    private void AoReceberOperacao(object? sender, EventArgs e)
        => MainThread.BeginInvokeOnMainThread(async () => await AtualizarAoVivoAsync());

    private async Task AtualizarAoVivoAsync()
    {
        if (EstaCarregando)
        {
            _atualizacaoPendente = true;
            return;
        }

        await CarregarResumoAsync(mostrarCarregamento: false);

        if (_atualizacaoPendente)
        {
            _atualizacaoPendente = false;
            await AtualizarAoVivoAsync();
        }
    }

    private async Task CarregarResumoAsync(bool mostrarCarregamento)
    {
        if (mostrarCarregamento)
        {
            await ExecutarAsync(async () => Aplicar(await BuscarResumoAsync()));
            return;
        }

        try
        {
            Aplicar(await BuscarResumoAsync());
            AoVivo = true;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            // Intervalo sem rede: mantém os últimos números na tela.
            AoVivo = false;
        }
        catch (Exception)
        {
            // Falha pontual na atualização automática: a próxima operação tenta de novo.
        }
    }

    private Task<ResumoEventoDTO> BuscarResumoAsync()
        => Api.GetAsync<ResumoEventoDTO>($"api/eventos/{EventoId}/relatorios");

    private void Aplicar(ResumoEventoDTO resumo)
    {
        Situacao = resumo.Situacao;
        QuantidadeRecargas = resumo.QuantidadeRecargas;
        TotalRecargas = resumo.TotalRecargas;
        QuantidadeVendas = resumo.QuantidadeVendas;
        TotalVendas = resumo.TotalVendas;
        SaldoEmCirculacao = resumo.SaldoEmCirculacao;

        PorSetor.Clear();
        foreach (var setor in resumo.PorSetor)
            PorSetor.Add(setor);

        PorOperador.Clear();
        foreach (var operador in resumo.PorOperador)
            PorOperador.Add(operador);

        PorProduto.Clear();
        foreach (var produto in resumo.PorProduto)
            PorProduto.Add(produto);

        TemPorSetor = PorSetor.Count > 0;
        TemPorOperador = PorOperador.Count > 0;
        TemPorProduto = PorProduto.Count > 0;

        TextoAtualizacao = $"Atualizado às {DateTime.Now:HH:mm}";
    }
}
