using AgapeApp.Mobile.Config;
using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.PrepararAsync();
    }

    /// <summary>Atalho de desenvolvimento: aponta o app para outro servidor da API.</summary>
    private async void AoConfigurarServidor(object? sender, EventArgs e)
    {
        var url = await DisplayPromptAsync(
            "Servidor",
            "Endereço da API (ex.: http://192.168.0.10:5180)",
            initialValue: AppConfig.ApiBaseUrl,
            keyboard: Keyboard.Url);

        if (!string.IsNullOrWhiteSpace(url))
        {
            AppConfig.DefinirApiBaseUrl(url);
            await DisplayAlertAsync("Pronto", $"Endereço salvo: {AppConfig.ApiBaseUrl}", "OK");
        }
    }
}
