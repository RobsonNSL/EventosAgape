using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class OperacaoEventoPage : ContentPage, IQueryAttributable
{
    private readonly OperacaoEventoViewModel _vm;

    public OperacaoEventoPage(OperacaoEventoViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CarregarRecentesAsync();
    }
}
