using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class ProdutosPage : ContentPage
{
    private readonly ProdutosViewModel _vm;

    public ProdutosPage(ProdutosViewModel vm)
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
