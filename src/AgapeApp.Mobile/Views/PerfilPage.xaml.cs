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
        _vm.Carregar();
        await _vm.CarregarFotoAsync();
        await _vm.PrepararBiometriaAsync();
    }

    private async void AoSair(object? sender, TappedEventArgs e)
    {
        var confirmar = await DisplayAlertAsync(
            "Sair da conta",
            "Deseja sair? Você precisará entrar de novo com e-mail e senha.",
            "Sair",
            "Cancelar");

        if (confirmar)
            await _vm.SairAsync();
    }
}
