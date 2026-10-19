using AgapeApp.Mobile.Config;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.ViewModels;
using AgapeApp.Mobile.Views.Janelas;

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

        if (Dialogos.Aberto)
            return;

        var preparo = _vm.PrepararAsync();
        await EsconderAberturaAsync(preparo);
        await preparo;

        // Com a biometria ativada, já pede a digital ao abrir (uma vez; depois fica o botão).
        if (_vm.BiometriaDisponivel && !_biometriaPedida)
        {
            _biometriaPedida = true;
            await _vm.EntrarComBiometriaCommand.ExecuteAsync(null);
        }
    }

    private bool _biometriaPedida;

    /// <summary>A foto de abertura só aparece uma vez por execução do app.</summary>
    private static bool _aberturaMostrada;

    private async Task EsconderAberturaAsync(Task preparo)
    {
        if (_aberturaMostrada || !Abertura.IsVisible)
        {
            Abertura.IsVisible = false;
            return;
        }

        _aberturaMostrada = true;

        try
        {
            // Tempo mínimo para a foto ser vista, sem segurar a pessoa além do preparo.
            await Task.WhenAll(preparo, Task.Delay(1100));
            await Abertura.FadeToAsync(0, 350, Easing.CubicIn);
        }
        catch (Exception)
        {
            // Sem animação: some de uma vez.
        }

        Abertura.IsVisible = false;
    }

    private int _toquesBrasao;

    /// <summary>5 toques no brasão liberam o atalho técnico de servidor (útil no Release).</summary>
    private void AoTocarBrasao(object? sender, TappedEventArgs e)
    {
        if (++_toquesBrasao < 5 || _vm.MostrarConfiguracaoServidor)
            return;

        _vm.MostrarConfiguracaoServidor = true;
        Notificacao.Mostrar("Configuração do servidor liberada");
    }

    /// <summary>Atalho técnico: aponta o app para outro servidor da API.</summary>
    private async void AoConfigurarServidor(object? sender, EventArgs e)
    {
        var url = await Dialogos.PerguntarAsync(
            "Servidor da API",
            "Endereço completo, com https (ex.: https://xxxx.ngrok-free.dev).",
            new OpcoesEntrada
            {
                ValorInicial = AppConfig.ApiBaseUrl,
                Teclado = Keyboard.Url,
                TamanhoMaximo = 200,
                Icone = "\uf233",
                Validar = texto => Uri.TryCreate(texto, UriKind.Absolute, out var uri)
                                   && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                    ? null
                    : "Informe um endereço válido começando com https://"
            });

        if (url is null)
            return;

        AppConfig.DefinirApiBaseUrl(url);
        Notificacao.Mostrar($"Servidor: {AppConfig.ApiBaseUrl}");
    }
}
