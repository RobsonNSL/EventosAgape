using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class SaldosPage : ContentPage, IQueryAttributable
{
    private readonly SaldosViewModel _vm;

    public SaldosPage(SaldosViewModel vm)
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
