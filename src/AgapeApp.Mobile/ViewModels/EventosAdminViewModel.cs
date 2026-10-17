using System.Collections.ObjectModel;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Eventos do app (administrador): listar, criar e abrir os detalhes de cada evento.</summary>
public partial class EventosAdminViewModel : BaseViewModel
{
    public EventosAdminViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ObservableCollection<EventoAdminItem> Eventos { get; } = [];

    public Task IniciarAsync() => ExecutarAsync(CarregarAsync);

    [RelayCommand]
    private Task AtualizarAsync() => IniciarAsync();

    [RelayCommand]
    private Task NovoAsync() => NavegarAsync("evento-form");

    [RelayCommand]
    private async Task AbrirAsync(EventoAdminItem? evento)
    {
        if (evento is null)
            return;

        await NavegarAsync("evento-detalhe", "evento", evento);
    }

    private async Task CarregarAsync()
    {
        var eventos = await Api.GetAsync<List<EventoDTO>>("api/eventos");

        Eventos.Clear();

        foreach (var evento in eventos.OrderByDescending(item => item.CriadoEm).Select(EventoAdminItem.De))
            Eventos.Add(evento);
    }
}
