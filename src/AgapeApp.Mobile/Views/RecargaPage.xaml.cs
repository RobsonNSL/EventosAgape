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

    private async void AoPedirOutroValor(object? sender, EventArgs e)
    {
        var texto = await DisplayPromptAsync(
            "Outro valor",
            "Digite o valor da recarga (R$):",
            placeholder: "15,50",
            keyboard: Keyboard.Numeric);

        if (!string.IsNullOrWhiteSpace(texto))
            _vm.DefinirValorPersonalizado(texto);
    }
}
