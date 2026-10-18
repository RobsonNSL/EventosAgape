using AgapeApp.Mobile.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace AgapeApp.Mobile.Views.Janelas;

/// <summary>
/// Seleção em lista com busca: escolha única (o toque já confirma) ou múltipla
/// (marca/desmarca e confirma no botão). Devolve os índices escolhidos ou nulo.
/// As linhas são montadas uma única vez; a busca só mostra/esconde, sem recriar nada.
/// </summary>
public sealed class SelecaoDialogo : DialogoBase<IReadOnlyList<int>?>
{
    private const int MinimoParaBusca = 6;

    private readonly bool _multipla;
    private readonly HashSet<int> _marcados;
    private readonly List<(int Indice, ItemSelecao Item, Border Linha, Label Marca)> _linhas = [];
    private readonly Label _vazio;
    private readonly Button? _confirmar;
    private readonly string _textoConfirmar;

    public SelecaoDialogo(
        string titulo,
        string? subtitulo,
        IReadOnlyList<ItemSelecao> itens,
        bool multipla,
        IEnumerable<int>? marcados,
        string textoConfirmar,
        string? textoVazio)
    {
        _multipla = multipla;
        _marcados = new HashSet<int>(marcados ?? Enumerable.Empty<int>());
        _textoConfirmar = textoConfirmar;

        var pilha = new VerticalStackLayout { Spacing = 6 };

        pilha.Add(UiDialogo.Titulo(titulo));

        var mensagem = UiDialogo.Mensagem(subtitulo);
        mensagem.FontSize = 14;
        pilha.Add(mensagem);

        if (itens.Count > MinimoParaBusca)
            pilha.Add(MontarBusca());

        var lista = new VerticalStackLayout { Spacing = 8 };

        for (var i = 0; i < itens.Count; i++)
            lista.Add(MontarLinha(i, itens[i]));

        _vazio = new Label
        {
            Text = textoVazio ?? "Nada encontrado.",
            FontFamily = "InterMedium",
            FontSize = 14,
            TextColor = Cores.TextoSecundario,
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 18),
            IsVisible = itens.Count == 0
        };
        lista.Add(_vazio);

        pilha.Add(new ScrollView
        {
            Content = lista,
            Margin = new Thickness(0, 8, 0, 0),
            MaximumHeightRequest = AlturaTela * 0.5,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
        });

        var botoes = new Grid
        {
            ColumnDefinitions = multipla
                ? new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star))
                : new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star)),
            ColumnSpacing = 10,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var cancelar = UiDialogo.BotaoContorno("Cancelar");
        cancelar.Clicked += (_, _) => Cancelar();
        botoes.Add(cancelar);

        if (multipla)
        {
            _confirmar = UiDialogo.Botao(textoConfirmar, TomDialogo.Primario);
            _confirmar.Clicked += (_, _) => Seguro(() => FecharAsync(_marcados.Order().ToList()));
            Grid.SetColumn(_confirmar, 1);
            botoes.Add(_confirmar);
            AtualizarBotao();
        }

        pilha.Add(botoes);

        DefinirCorpo(pilha);
    }

    protected override IReadOnlyList<int>? ResultadoCancelado => null;

    private Border MontarBusca()
    {
        var campo = new Entry
        {
            Placeholder = "Buscar",
            FontSize = 15,
            ReturnType = ReturnType.Search
        };
        campo.TextChanged += (_, e) => Filtrar(e.NewTextValue);

        var grade = new Grid
        {
            ColumnDefinitions = [new ColumnDefinition(20), new ColumnDefinition(GridLength.Star)],
            ColumnSpacing = 8
        };
        grade.Add(new Label
        {
            Text = "\uf002",
            FontFamily = "fa",
            FontSize = 14,
            TextColor = Cores.TextoMudo,
            VerticalOptions = LayoutOptions.Center
        });
        Grid.SetColumn(campo, 1);
        grade.Add(campo);

        return new Border
        {
            BackgroundColor = Cores.Superficie,
            Stroke = Cores.Borda,
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(14, 0),
            HeightRequest = 48,
            Margin = new Thickness(0, 8, 0, 0),
            Content = grade
        };
    }

    private Border MontarLinha(int indice, ItemSelecao item)
    {
        var grade = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(42),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(26)
            ],
            ColumnSpacing = 12
        };

        View avatar = item.Iniciais is { Length: > 0 } iniciais
            ? new Label
            {
                Text = iniciais,
                FontFamily = "InterSemiBold",
                FontSize = 14,
                TextColor = Cores.AzulTexto,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
            : new Label
            {
                Text = item.Icone,
                FontFamily = "fa",
                FontSize = 15,
                TextColor = Cores.Primaria,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

        grade.Add(new Border
        {
            WidthRequest = 40,
            HeightRequest = 40,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Cores.AzulFundo,
            VerticalOptions = LayoutOptions.Center,
            Content = avatar
        });

        var textos = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        textos.Add(new Label
        {
            Text = item.Titulo,
            FontFamily = "InterSemiBold",
            FontSize = 15,
            TextColor = Cores.TextoPrimario,
            LineBreakMode = LineBreakMode.TailTruncation
        });

        if (!string.IsNullOrWhiteSpace(item.Detalhe))
        {
            textos.Add(new Label
            {
                Text = item.Detalhe,
                FontFamily = "InterMedium",
                FontSize = 12,
                TextColor = Cores.TextoSecundario,
                LineBreakMode = LineBreakMode.TailTruncation
            });
        }

        Grid.SetColumn(textos, 1);
        grade.Add(textos);

        var marca = new Label
        {
            FontFamily = "fa",
            FontSize = 20,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(marca, 2);
        grade.Add(marca);

        var linha = new Border
        {
            Stroke = Cores.Borda,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(12, 10),
            Content = grade
        };

        var toque = new TapGestureRecognizer();
        toque.Tapped += (_, _) => Seguro(() => TocarAsync(indice));
        linha.GestureRecognizers.Add(toque);

        _linhas.Add((indice, item, linha, marca));
        PintarLinha(linha, marca, _marcados.Contains(indice));

        return linha;
    }

    private async Task TocarAsync(int indice)
    {
        var linha = _linhas.First(l => l.Indice == indice);

        if (!_multipla)
        {
            PintarLinha(linha.Linha, linha.Marca, true);
            await UiDialogo.PulsarAsync(linha.Linha);
            await FecharAsync([indice]);
            return;
        }

        if (!_marcados.Remove(indice))
            _marcados.Add(indice);

        PintarLinha(linha.Linha, linha.Marca, _marcados.Contains(indice));
        AtualizarBotao();
    }

    private void PintarLinha(Border linha, Label marca, bool marcado)
    {
        linha.BackgroundColor = marcado ? Cores.SuperficieTint : Cores.Superficie;
        linha.Stroke = marcado ? Cores.Primaria : Cores.Borda;

        if (_multipla)
        {
            marca.Text = marcado ? "\uf14a" : "\uf0c8";
            marca.TextColor = marcado ? Cores.Primaria : Cores.Borda;
        }
        else
        {
            marca.Text = "\uf054";
            marca.FontSize = 13;
            marca.TextColor = Cores.TextoMudo;
        }
    }

    private void AtualizarBotao()
    {
        if (_confirmar is null)
            return;

        _confirmar.Text = _marcados.Count > 0 ? $"{_textoConfirmar} ({_marcados.Count})" : _textoConfirmar;
    }

    private void Filtrar(string? termo)
    {
        var visiveis = 0;

        foreach (var (_, item, linha, _) in _linhas)
        {
            var mostra = Formatos.Contem(item.Titulo, termo) || Formatos.Contem(item.Detalhe, termo);
            linha.IsVisible = mostra;

            if (mostra)
                visiveis++;
        }

        _vazio.IsVisible = visiveis == 0;
    }
}
