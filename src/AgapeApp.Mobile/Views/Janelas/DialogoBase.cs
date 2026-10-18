using AgapeApp.Mobile.Helpers;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Shapes;

namespace AgapeApp.Mobile.Views.Janelas;

/// <summary>
/// Base das janelas do app (confirmação, opções, campo e seleção): página modal
/// transparente com o fundo escurecido e uma "folha" que sobe da parte de baixo da tela.
/// Substitui os alertas nativos (DisplayAlert/ActionSheet/Prompt), que no Android
/// abriam uma caixa escura e, encadeados, podiam deixar a tela sem resposta.
/// </summary>
public abstract class DialogoBase<TResultado> : ContentPage
{
    private readonly TaskCompletionSource<TResultado> _conclusao =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly BoxView _fundo;
    private readonly Border _folha;
    private readonly bool _centralizado;

    private bool _apresentado;
    private bool _fechando;

    protected DialogoBase(bool centralizado = false)
    {
        _centralizado = centralizado;

        BackgroundColor = Colors.Transparent;
        Shell.SetNavBarIsVisible(this, false);

        // iOS: sem isto a página modal cobre a tela inteira e o fundo fica preto.
        On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

        _fundo = new BoxView { Color = Cores.TextoPrimario, Opacity = 0 };

        var toqueFundo = new TapGestureRecognizer();
        toqueFundo.Tapped += (_, _) => Cancelar();
        _fundo.GestureRecognizers.Add(toqueFundo);

        _folha = new Border
        {
            BackgroundColor = Cores.Superficie,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = centralizado ? new CornerRadius(20) : new CornerRadius(24, 24, 0, 0)
            },
            Padding = centralizado ? new Thickness(20) : new Thickness(20, 10, 20, 28),
            VerticalOptions = centralizado ? LayoutOptions.Start : LayoutOptions.End,
            Margin = centralizado ? new Thickness(16, AlturaTela * 0.12, 16, 0) : Thickness.Zero,
            MaximumWidthRequest = 560
        };

        Content = new Grid { Children = { _fundo, _folha } };
    }

    /// <summary>Resultado escolhido pelo usuário (ou o valor de cancelamento).</summary>
    public Task<TResultado> Resultado => _conclusao.Task;

    /// <summary>Valor devolvido ao tocar fora da folha ou no botão voltar do aparelho.</summary>
    protected abstract TResultado ResultadoCancelado { get; }

    /// <summary>Altura útil da tela em unidades independentes de densidade.</summary>
    protected static double AlturaTela
    {
        get
        {
            var tela = DeviceDisplay.Current.MainDisplayInfo;
            return tela.Density > 0 ? tela.Height / tela.Density : 800;
        }
    }

    /// <summary>Define o conteúdo da folha (a alça de arrastar é incluída automaticamente).</summary>
    protected void DefinirCorpo(View corpo)
    {
        if (_centralizado)
        {
            _folha.Content = corpo;
            return;
        }

        var alca = new BoxView
        {
            Color = Cores.Borda,
            WidthRequest = 40,
            HeightRequest = 4,
            CornerRadius = 2,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 14)
        };

        _folha.Content = new VerticalStackLayout { Children = { alca, corpo } };
    }

    /// <summary>Chamado depois da animação de entrada (ex.: focar o campo de texto).</summary>
    protected virtual void AoApresentar()
    {
    }

    /// <summary>Chamado no início do fechamento (ex.: desligar a câmera).</summary>
    protected virtual void AoFechar()
    {
    }

    protected void Cancelar() => _ = FecharAsync(ResultadoCancelado);

    protected async Task FecharAsync(TResultado resultado)
    {
        if (_fechando)
            return;

        _fechando = true;

        try
        {
            AoFechar();
        }
        catch (Exception erro)
        {
            RegistroErros.Registrar(erro, "FecharJanela");
        }

        try
        {
            await Task.WhenAll(
                _fundo.FadeToAsync(0, 140),
                _centralizado
                    ? _folha.FadeToAsync(0, 140)
                    : _folha.TranslateToAsync(0, Math.Max(_folha.Height, 320), 170, Easing.CubicIn));
        }
        catch (Exception)
        {
            // Animação interrompida: fecha do mesmo jeito.
        }

        try
        {
            // Teclado aberto durante o fechamento da janela já derrubou o app no Android.
            EsconderTeclado(Content);

            if (Navigation.ModalStack.Contains(this))
                await Navigation.PopModalAsync(false);
        }
        catch (Exception erro)
        {
            // Nunca deixa o fechamento derrubar o app: registra e entrega o resultado.
            RegistroErros.Registrar(erro, "FecharJanela");
        }
        finally
        {
            _conclusao.TrySetResult(resultado);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_apresentado)
            return;

        _apresentado = true;

        if (_centralizado)
        {
            _folha.Opacity = 0;
            _folha.Scale = 0.94;
        }
        else
        {
            _folha.TranslationY = AlturaTela;
        }

        try
        {
            await Task.WhenAll(
                _fundo.FadeToAsync(0.55, 180),
                _centralizado
                    ? Task.WhenAll(_folha.FadeToAsync(1, 180), _folha.ScaleToAsync(1, 180, Easing.CubicOut))
                    : _folha.TranslateToAsync(0, 0, 230, Easing.CubicOut));
        }
        catch (Exception)
        {
            _folha.TranslationY = 0;
            _folha.Opacity = 1;
            _folha.Scale = 1;
        }

        try
        {
            AoApresentar();
        }
        catch (Exception erro)
        {
            RegistroErros.Registrar(erro, "AbrirJanela");
        }
    }

    /// <summary>
    /// Executa a ação de um botão/toque da janela sem deixar exceção escapar
    /// (em evento async void, uma exceção solta fecha o app).
    /// </summary>
    protected static async void Seguro(Func<Task> acao)
    {
        try
        {
            await acao();
        }
        catch (Exception erro)
        {
            RegistroErros.Registrar(erro, "Janela");
        }
    }

    private static void EsconderTeclado(IView? raiz)
    {
        switch (raiz)
        {
            case InputView campo when campo.IsFocused:
                campo.Unfocus();
                break;

            case Layout layout:
                foreach (var filho in layout)
                    EsconderTeclado(filho);
                break;

            case Border borda:
                EsconderTeclado(borda.Content);
                break;

            case Microsoft.Maui.Controls.ScrollView rolagem:
                EsconderTeclado(rolagem.Content);
                break;
        }
    }

    /// <summary>Botão voltar do Android fecha a janela em vez de sair da tela de trás.</summary>
    protected override bool OnBackButtonPressed()
    {
        Cancelar();
        return true;
    }
}
