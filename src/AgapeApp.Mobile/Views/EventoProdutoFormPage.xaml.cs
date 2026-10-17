using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class EventoProdutoFormPage : ContentPage, IQueryAttributable
{
    private readonly EventoProdutoFormViewModel _vm;

    public EventoProdutoFormPage(EventoProdutoFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);
}
