using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class PerfilPage : ContentPage
{
    private readonly PerfilViewModel _vm;

    public PerfilPage(PerfilViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Dialogos.Aberto)
            return;

        AnimarEntrada();

        _vm.Carregar();
        await _vm.CarregarFotoAsync();
        await _vm.PrepararBiometriaAsync();
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
