using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class VendaConcluidaPage : ContentPage, IQueryAttributable
{
    private readonly VendaConcluidaViewModel _vm;
    private bool _comemorado;

    public VendaConcluidaPage(VendaConcluidaViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);

    /// <summary>Comemoração uma vez só: ✓ com mola, onda, confete e os valores contando.</summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_comemorado)
            return;

        _comemorado = true;

        _ = Animacoes.CascataAsync(Detalhes, 70);
        await Animacoes.CelebrarAsync(Circulo, Check, Onda, Palco);
        await Task.WhenAll(
            Animacoes.ContarAsync(RotuloTotal, _vm.Total),
            Animacoes.ContarAsync(RotuloSaldo, _vm.SaldoAtual, 900));
        await Animacoes.PulsarAsync(RotuloSaldo);
    }
}
