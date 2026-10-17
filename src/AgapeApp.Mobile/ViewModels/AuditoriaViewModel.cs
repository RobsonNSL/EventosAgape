using System.Collections.ObjectModel;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Auditoria do evento (administrador): registros paginados, mais recentes primeiro.</summary>
public partial class AuditoriaViewModel : BaseViewModel
{
    private const int TamanhoPagina = 25;

    public AuditoriaViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ObservableCollection<LogAuditoriaItem> Registros { get; } = [];

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoCarregarMais))]
    private int total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoCarregarMais))]
    private bool temMais;

    [ObservableProperty]
    private string textoTotal = "Carregando registros...";

    private int _pagina;

    public string TextoCarregarMais => $"Carregar mais ({Registros.Count} de {Total})";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        EventoId = Parametros.Guid(parametros, "eventoId");
        EventoNome = Parametros.Texto(parametros, "eventoNome");
    }

    public async Task IniciarAsync() => await CarregarAsync(reiniciar: true);

    [RelayCommand]
    private Task AtualizarAsync() => CarregarAsync(reiniciar: true);

    [RelayCommand]
    private Task CarregarMaisAsync() => CarregarAsync(reiniciar: false);

    private async Task CarregarAsync(bool reiniciar)
    {
        await ExecutarAsync(async () =>
        {
            if (reiniciar)
            {
                _pagina = 0;
                Registros.Clear();
            }

            var proxima = _pagina + 1;
            var resultado = await Api.GetAsync<ResultadoPaginadoDTO<LogAuditoriaDTO>>(
                $"api/eventos/{EventoId}/auditoria?pagina={proxima}&tamanhoPagina={TamanhoPagina}");

            _pagina = proxima;
            Total = resultado.Total;

            foreach (var log in resultado.Itens)
                Registros.Add(LogAuditoriaItem.De(log));

            TemMais = Registros.Count < Total;
            TextoTotal = Total == 1 ? "1 registro" : $"{Total} registros";
            OnPropertyChanged(nameof(TextoCarregarMais));
        });
    }
}
