using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class CartoesPage : ContentPage
{
    private readonly CartoesViewModel _vm;

    public CartoesPage(CartoesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.IniciarAsync();
    }
}
