using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class VendaPage : ContentPage, IQueryAttributable
{
    private readonly VendaViewModel _vm;

    public VendaPage(VendaViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        // O total "pulsa" a cada item somado ou tirado.
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(VendaViewModel.TextoTotal) && _vm.TemItens)
                _ = Animacoes.PulsarAsync(RotuloTotal, 1.08);
        };
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.IniciarAsync();
    }
}
