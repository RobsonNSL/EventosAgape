using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class EventoDetalhePage : ContentPage, IQueryAttributable
{
    private readonly EventoDetalheViewModel _vm;

    public EventoDetalhePage(EventoDetalheViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);

    // Só busca de novo quando algo mudou (ex.: voltou do formulário de produto);
    // fechar uma janela de escolha não recarrega a tela inteira.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.IniciarAsync();
    }
}
