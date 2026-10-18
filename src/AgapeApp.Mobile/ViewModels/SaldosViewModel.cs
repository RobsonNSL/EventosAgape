using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Saldos em circulação do evento (administrador): cartões com saldo disponível.</summary>
public partial class SaldosViewModel : BaseViewModel
{
    public SaldosViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ColecaoObservavel<CartaoSaldoItem> Cartoes { get; } = [];

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoTotal))]
    private decimal total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoQuantidade))]
    private int quantidade;

    public string TextoTotal => Formatos.Moeda(Total);

    public string TextoQuantidade => Quantidade == 1
        ? "1 cartão com saldo"
        : $"{Quantidade} cartões com saldo";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
    }

    public async Task IniciarAsync()
    {
        await ExecutarAsync(async () =>
        {
            var saldos = await Api.GetAsync<List<SaldoEmCirculacaoDTO>>(
                $"api/eventos/{EventoId}/relatorios/saldos");

            Cartoes.Substituir(saldos.OrderByDescending(s => s.Saldo).Select(CartaoSaldoItem.De));

            Quantidade = Cartoes.Count;
            Total = saldos.Sum(s => s.Saldo);
        });
    }

    [RelayCommand]
    private Task AtualizarAsync() => IniciarAsync();
}
