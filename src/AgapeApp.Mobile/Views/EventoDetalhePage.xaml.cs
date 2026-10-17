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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.IniciarAsync();
    }
}
