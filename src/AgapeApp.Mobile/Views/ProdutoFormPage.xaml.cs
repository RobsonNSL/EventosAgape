using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class ProdutoFormPage : ContentPage, IQueryAttributable
{
    private readonly ProdutoFormViewModel _vm;

    public ProdutoFormPage(ProdutoFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);
}
