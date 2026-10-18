using AgapeApp.Mobile.Helpers;
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

    public ColecaoObservavel<EventoAdminItem> Eventos { get; } = [];

    public Task IniciarAsync()
        => PrecisaRecarregar() ? ExecutarAsync(CarregarAsync) : Task.CompletedTask;

    [RelayCommand]
    private Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

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

        // Abertos no topo, depois em preparação e por fim os encerrados.
        Eventos.Substituir(eventos
            .OrderBy(item => item.Situacao switch { "Aberto" => 0, "Rascunho" => 1, _ => 2 })
            .ThenByDescending(item => item.DataInicio)
            .Select(EventoAdminItem.De));

        MarcarComoAtualizado();
    }
}
