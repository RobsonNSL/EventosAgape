using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class VendaPage : ContentPage, IQueryAttributable
{
    private readonly VendaViewModel _vm;

    public VendaPage(VendaViewModel vm)
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
