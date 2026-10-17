using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class RecargaConcluidaPage : ContentPage, IQueryAttributable
{
    private readonly RecargaConcluidaViewModel _vm;

    public RecargaConcluidaPage(RecargaConcluidaViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);
}
