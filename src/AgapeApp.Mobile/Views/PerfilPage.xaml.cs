using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class PerfilPage : ContentPage
{
    private readonly PerfilViewModel _vm;

    public PerfilPage(PerfilViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Dialogos.Aberto)
            return;

        _vm.Carregar();
        await _vm.CarregarFotoAsync();
        await _vm.PrepararBiometriaAsync();
    }
}
