using AgapeApp.Mobile.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace AgapeApp.Mobile.Views.Janelas;

/// <summary>Opções do campo de texto da janela de entrada.</summary>
public sealed record OpcoesEntrada
{
    public string? Placeholder { get; init; }
    public string? ValorInicial { get; init; }
    public Keyboard? Teclado { get; init; }
    public int TamanhoMaximo { get; init; } = 100;
    public bool Senha { get; init; }
    public string TextoConfirmar { get; init; } = "Salvar";
    public string Icone { get; init; } = "\uf303";
    public TomDialogo Tom { get; init; } = TomDialogo.Primario;

    /// <summary>Valida o texto: devolve a mensagem de erro (a janela continua aberta) ou nulo.</summary>
    public Func<string, string?>? Validar { get; init; }
}

/// <summary>
/// Janela com um campo de texto (substitui o DisplayPromptAsync). Fica no alto da tela
/// para o teclado não cobrir o campo, valida antes de fechar e mostra o erro ali mesmo.
/// </summary>
public sealed class EntradaDialogo : DialogoBase<string?>
{
    private readonly Entry _campo;
    private readonly Label _erro;
    private readonly OpcoesEntrada _opcoes;

    public EntradaDialogo(string titulo, string? mensagem, OpcoesEntrada opcoes) : base(centralizado: true)
    {
        _opcoes = opcoes;

        var pilha = new VerticalStackLayout { Spacing = 12 };

        var cabecalho = new Grid
        {
            ColumnDefinitions = [new ColumnDefinition(52), new ColumnDefinition(GridLength.Star)],
            ColumnSpacing = 12
        };
        var icone = UiDialogo.Icone(opcoes.Icone, opcoes.Tom, 44);
        icone.VerticalOptions = LayoutOptions.Center;
        cabecalho.Add(icone);

        var tituloLabel = UiDialogo.Titulo(titulo);
        tituloLabel.FontSize = 18;
        tituloLabel.VerticalOptions = LayoutOptions.Center;
        Grid.SetColumn(tituloLabel, 1);
        cabecalho.Add(tituloLabel);
        pilha.Add(cabecalho);

        var texto = UiDialogo.Mensagem(mensagem);
        texto.FontSize = 14;
        pilha.Add(texto);

        _campo = new Entry
        {
            Text = opcoes.ValorInicial,
            Placeholder = opcoes.Placeholder,
            Keyboard = opcoes.Teclado ?? Keyboard.Default,
            MaxLength = opcoes.TamanhoMaximo,
            IsPassword = opcoes.Senha,
            ReturnType = ReturnType.Done,
            FontSize = 17
        };
        _campo.Completed += (_, _) => Seguro(ConfirmarAsync);
        _campo.TextChanged += (_, _) => _erro!.IsVisible = false;

        var linhaCampo = new Grid
        {
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)]
        };
        linhaCampo.Add(_campo);

        if (opcoes.Senha)
        {
            var olho = new Label
            {
                Text = "\uf070",
                FontFamily = "fa",
                FontSize = 16,
                TextColor = Cores.TextoMudo,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10, 8, 2, 8)
            };
            var toqueOlho = new TapGestureRecognizer();
            toqueOlho.Tapped += (_, _) =>
            {
                _campo.IsPassword = !_campo.IsPassword;
                olho.Text = _campo.IsPassword ? "\uf070" : "\uf06e";
            };
            olho.GestureRecognizers.Add(toqueOlho);
            Grid.SetColumn(olho, 1);
            linhaCampo.Add(olho);
        }

        pilha.Add(new Border
        {
            BackgroundColor = Cores.Superficie,
            Stroke = Cores.Primaria,
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(14, 0),
            HeightRequest = 56,
            Content = linhaCampo
        });

        _erro = new Label
        {
            FontFamily = "InterMedium",
            FontSize = 13,
            TextColor = Cores.Perigo,
            IsVisible = false
        };
        pilha.Add(_erro);

        var botoes = new Grid
        {
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)],
            ColumnSpacing = 10,
            Margin = new Thickness(0, 4, 0, 0)
        };

        var cancelar = UiDialogo.BotaoContorno("Cancelar");
        cancelar.Clicked += (_, _) => Cancelar();
        botoes.Add(cancelar);

        var confirmar = UiDialogo.Botao(opcoes.TextoConfirmar, opcoes.Tom);
        confirmar.Clicked += (_, _) => Seguro(ConfirmarAsync);
        Grid.SetColumn(confirmar, 1);
        botoes.Add(confirmar);

        pilha.Add(botoes);

        DefinirCorpo(pilha);
    }

    protected override string? ResultadoCancelado => null;

    /// <summary>
    /// Foca o campo um instante depois de a janela aparecer. Não mexe no CursorPosition:
    /// com texto já preenchido (ex.: próximo código de cartão sugerido), definir o cursor
    /// antes de o campo nativo receber o texto lança IndexOutOfBounds no Android e fecha o app.
    /// </summary>
    protected override void AoApresentar()
        => Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(120), () =>
        {
            try
            {
                _campo.Focus();
            }
            catch (Exception erro)
            {
                RegistroErros.Registrar(erro, "FocarCampo");
            }
        });

    private async Task ConfirmarAsync()
    {
        var texto = (_campo.Text ?? string.Empty).Trim();

        var erro = string.IsNullOrWhiteSpace(texto)
            ? "Preencha o campo para continuar."
            : _opcoes.Validar?.Invoke(texto);

        if (erro is not null)
        {
            _erro.Text = erro;
            _erro.IsVisible = true;
            return;
        }

        _campo.Unfocus();
        await FecharAsync(_opcoes.Senha ? _campo.Text : texto);
    }
}
