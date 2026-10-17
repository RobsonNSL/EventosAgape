using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class DashboardPage : ContentPage, IQueryAttributable
{
    private readonly DashboardViewModel _vm;

    public DashboardPage(DashboardViewModel vm)
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

    protected override async void OnDisappearing()
    {
        await _vm.PararAsync();
        base.OnDisappearing();
    }
}
