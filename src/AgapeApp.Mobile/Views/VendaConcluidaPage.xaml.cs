using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class VendaConcluidaPage : ContentPage, IQueryAttributable
{
    private readonly VendaConcluidaViewModel _vm;

    public VendaConcluidaPage(VendaConcluidaViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);
}
