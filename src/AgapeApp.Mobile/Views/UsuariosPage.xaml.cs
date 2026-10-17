using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class UsuariosPage : ContentPage
{
    private readonly UsuariosViewModel _vm;

    public UsuariosPage(UsuariosViewModel vm)
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
