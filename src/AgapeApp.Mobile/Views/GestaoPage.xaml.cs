using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class GestaoPage : ContentPage
{
    public GestaoPage(GestaoViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AnimarEntrada();
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
