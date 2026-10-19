using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class SaldoPage : ContentPage, IQueryAttributable
{
    private readonly SaldoViewModel _vm;

    public SaldoPage(SaldoViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_animado)
            return;

        _animado = true;
        _ = Animacoes.CascataAsync(Conteudo);

        await _vm.IniciarAsync();

        // Saldo "sobe" até o valor consultado.
        if (_vm.SaldoAtual is decimal saldo)
            await Animacoes.ContarAsync(RotuloSaldo, saldo, 800);
    }

    private bool _animado;
}
