using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class UsuarioFormPage : ContentPage, IQueryAttributable
{
    private readonly UsuarioFormViewModel _vm;

    public UsuarioFormPage(UsuarioFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);
}
