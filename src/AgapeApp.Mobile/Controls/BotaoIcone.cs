using System.Windows.Input;
using AgapeApp.Mobile.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace AgapeApp.Mobile.Controls;

/// <summary>
/// Botão quadrado com ícone (Font Awesome). O toque vale no quadro inteiro — antes o
/// gesto ficava só no glifo de ~15px e muitos toques "não pegavam".
/// </summary>
public class BotaoIcone : Border
{
    public static readonly BindableProperty GlifoProperty = BindableProperty.Create(
        nameof(Glifo), typeof(string), typeof(BotaoIcone), string.Empty,
        propertyChanged: (b, _, novo) => ((BotaoIcone)b)._icone.Text = (string?)novo);

    public static readonly BindableProperty CorIconeProperty = BindableProperty.Create(
        nameof(CorIcone), typeof(Color), typeof(BotaoIcone), Cores.TextoPrimario,
        propertyChanged: (b, _, novo) => ((BotaoIcone)b)._icone.TextColor = (Color)novo);

    public static readonly BindableProperty TamanhoIconeProperty = BindableProperty.Create(
        nameof(TamanhoIcone), typeof(double), typeof(BotaoIcone), 15d,
        propertyChanged: (b, _, novo) => ((BotaoIcone)b)._icone.FontSize = (double)novo);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(BotaoIcone));

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(BotaoIcone));

    private readonly Label _icone;

    public BotaoIcone()
    {
        WidthRequest = 44;
        HeightRequest = 44;
        Stroke = Cores.Borda;
        StrokeThickness = 1;
        StrokeShape = new RoundRectangle { CornerRadius = 12 };
        BackgroundColor = Cores.Superficie;

        _icone = new Label
        {
            FontFamily = "fa",
            FontSize = TamanhoIcone,
            TextColor = CorIcone,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true
        };

        Content = _icone;

        var toque = new TapGestureRecognizer();
        toque.Tapped += AoTocar;
        GestureRecognizers.Add(toque);
    }

    /// <summary>Disparado no toque (alternativa ao Command para código da página).</summary>
    public event EventHandler? Tocado;

    public string Glifo
    {
        get => (string)GetValue(GlifoProperty);
        set => SetValue(GlifoProperty, value);
    }

    public Color CorIcone
    {
        get => (Color)GetValue(CorIconeProperty);
        set => SetValue(CorIconeProperty, value);
    }

    public double TamanhoIcone
    {
        get => (double)GetValue(TamanhoIconeProperty);
        set => SetValue(TamanhoIconeProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected virtual void AoExecutar()
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }

    private async void AoTocar(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled)
            return;

        AoExecutar();
        Tocado?.Invoke(this, EventArgs.Empty);

        try
        {
            await this.ScaleToAsync(0.9, 50);
            await this.ScaleToAsync(1, 70);
        }
        catch (Exception)
        {
            Scale = 1;
        }
    }
}

/// <summary>Botão "voltar" do cabeçalho das telas (sem Command, volta uma tela).</summary>
public class BotaoVoltar : BotaoIcone
{
    public BotaoVoltar()
    {
        Glifo = "\uf060";
        WidthRequest = 40;
        HeightRequest = 40;
        VerticalOptions = LayoutOptions.Start;
    }

    protected override void AoExecutar()
    {
        if (Command is not null)
        {
            base.AoExecutar();
            return;
        }

        if (Shell.Current is not null)
            _ = Shell.Current.GoToAsync("..");
    }
}
