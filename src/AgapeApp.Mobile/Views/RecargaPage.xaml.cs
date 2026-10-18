using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class RecargaPage : ContentPage, IQueryAttributable
{
    private readonly RecargaViewModel _vm;

    public RecargaPage(RecargaViewModel vm)
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
