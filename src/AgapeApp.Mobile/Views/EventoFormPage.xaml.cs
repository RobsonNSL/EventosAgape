using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class EventoFormPage : ContentPage, IQueryAttributable
{
    private readonly EventoFormViewModel _vm;

    public EventoFormPage(EventoFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);
}
