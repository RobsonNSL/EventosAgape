using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class EventosAdminPage : ContentPage
{
    private readonly EventosAdminViewModel _vm;

    public EventosAdminPage(EventosAdminViewModel vm)
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
