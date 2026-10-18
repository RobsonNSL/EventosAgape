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

        await _vm.PrepararAsync();

        // Com a biometria ativada, já pede a digital ao abrir (uma vez; depois fica o botão).
        if (_vm.BiometriaDisponivel && !_biometriaPedida)
        {
            _biometriaPedida = true;
            await _vm.EntrarComBiometriaCommand.ExecuteAsync(null);
        }
    }

    private bool _biometriaPedida;

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
