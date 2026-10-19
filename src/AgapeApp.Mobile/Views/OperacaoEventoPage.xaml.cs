using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class OperacaoEventoPage : ContentPage, IQueryAttributable
{
    private readonly OperacaoEventoViewModel _vm;

    public OperacaoEventoPage(OperacaoEventoViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AnimarEntrada();
        await _vm.CarregarRecentesAsync();
    }

    private bool _animado;

    /// <summary>Os cards entram em cascata só na primeira vez que a tela aparece.</summary>
    private void AnimarEntrada()
    {
        if (_animado)
            return;

        _animado = true;
        _ = Animacoes.CascataAsync(Conteudo);
    }

}
