using AgapeApp.Mobile.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace AgapeApp.Mobile.Views.Janelas;

/// <summary>
/// Lista de ações (substitui o DisplayActionSheet): cada opção com ícone, título e
/// uma descrição curta do que acontece. Devolve a chave da opção tocada ou nulo.
/// </summary>
public sealed class OpcoesDialogo : DialogoBase<string?>
{
    public OpcoesDialogo(string titulo, string? subtitulo, IReadOnlyList<OpcaoDialogo> opcoes)
    {
        var pilha = new VerticalStackLayout { Spacing = 6 };

        pilha.Add(UiDialogo.Titulo(titulo));

        var mensagem = UiDialogo.Mensagem(subtitulo);
        mensagem.FontSize = 14;
        pilha.Add(mensagem);

        var lista = new VerticalStackLayout { Spacing = 8, Margin = new Thickness(0, 10, 0, 0) };

        foreach (var opcao in opcoes)
            lista.Add(MontarOpcao(opcao));

        pilha.Add(new ScrollView
        {
            Content = lista,
            MaximumHeightRequest = AlturaTela * 0.6,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
        });

        var cancelar = UiDialogo.BotaoContorno("Cancelar");
        cancelar.Margin = new Thickness(0, 10, 0, 0);
        cancelar.Clicked += (_, _) => Cancelar();
        pilha.Add(cancelar);

        DefinirCorpo(pilha);
    }

    protected override string? ResultadoCancelado => null;

    private Border MontarOpcao(OpcaoDialogo opcao)
    {
        var grade = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(44),
                new ColumnDefinition(GridLength.Star)
            ],
            ColumnSpacing = 12
        };

        var icone = UiDialogo.Icone(opcao.Icone, opcao.Tom, 40);
        icone.VerticalOptions = LayoutOptions.Center;
        grade.Add(icone);

        var textos = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        textos.Add(new Label
        {
            Text = opcao.Titulo,
            FontFamily = "InterSemiBold",
            FontSize = 16,
            TextColor = opcao.Tom == TomDialogo.Perigo ? Cores.Perigo : Cores.TextoPrimario
        });

        if (!string.IsNullOrWhiteSpace(opcao.Descricao))
        {
            textos.Add(new Label
            {
                Text = opcao.Descricao,
                FontFamily = "InterMedium",
                FontSize = 13,
                TextColor = Cores.TextoSecundario
            });
        }

        Grid.SetColumn(textos, 1);
        grade.Add(textos);

        var linha = new Border
        {
            BackgroundColor = Cores.Superficie,
            Stroke = Cores.Borda,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(12),
            Content = grade
        };

        var toque = new TapGestureRecognizer();
        toque.Tapped += (_, _) => Seguro(async () =>
        {
            linha.BackgroundColor = Cores.SuperficieTint;
            await UiDialogo.PulsarAsync(linha);
            await FecharAsync(opcao.Chave);
        });
        linha.GestureRecognizers.Add(toque);

        return linha;
    }
}
