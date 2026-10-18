using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class MeusEventosPage : ContentPage
{
    private readonly MeusEventosViewModel _vm;

    public MeusEventosPage(MeusEventosViewModel vm)
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
