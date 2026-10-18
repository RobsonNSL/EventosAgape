using AgapeApp.Mobile.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace AgapeApp.Mobile.Views.Janelas;

/// <summary>
/// Confirmação (ou aviso, quando não há botão de cancelar): ícone, título, mensagem,
/// lista opcional de verificações e os botões empilhados — fáceis de alcançar com o polegar.
/// </summary>
public sealed class ConfirmacaoDialogo : DialogoBase<bool>
{
    public ConfirmacaoDialogo(
        string titulo,
        string? mensagem,
        string textoConfirmar,
        string? textoCancelar,
        TomDialogo tom,
        string icone,
        IReadOnlyList<ItemChecagem>? itens = null)
    {
        var pilha = new VerticalStackLayout { Spacing = 12 };

        pilha.Add(UiDialogo.Icone(icone, tom));
        pilha.Add(UiDialogo.Titulo(titulo));
        pilha.Add(UiDialogo.Mensagem(mensagem));

        if (itens is { Count: > 0 })
            pilha.Add(MontarChecagem(itens));

        var confirmar = UiDialogo.Botao(textoConfirmar, tom);
        confirmar.Margin = new Thickness(0, 8, 0, 0);
        confirmar.Clicked += (_, _) => Seguro(() => FecharAsync(true));
        pilha.Add(confirmar);

        if (!string.IsNullOrWhiteSpace(textoCancelar))
        {
            var cancelar = UiDialogo.BotaoContorno(textoCancelar);
            cancelar.Clicked += (_, _) => Seguro(() => FecharAsync(false));
            pilha.Add(cancelar);
        }

        DefinirCorpo(pilha);
    }

    protected override bool ResultadoCancelado => false;

    private static Border MontarChecagem(IReadOnlyList<ItemChecagem> itens)
    {
        var linhas = new VerticalStackLayout { Spacing = 10 };

        foreach (var item in itens)
        {
            var linha = new Grid
            {
                ColumnDefinitions = [new ColumnDefinition(22), new ColumnDefinition(GridLength.Star)],
                ColumnSpacing = 8
            };

            linha.Add(new Label
            {
                Text = item.Ok ? "\uf058" : "\uf071",
                FontFamily = "fa",
                FontSize = 14,
                TextColor = item.Ok ? Cores.Sucesso : Cores.AmbarTexto,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 2, 0, 0)
            });

            var texto = new Label
            {
                Text = item.Texto,
                FontFamily = "InterMedium",
                FontSize = 14,
                TextColor = Cores.TextoPrimario
            };
            Grid.SetColumn(texto, 1);
            linha.Add(texto);

            linhas.Add(linha);
        }

        return new Border
        {
            BackgroundColor = Cores.Fundo,
            Stroke = Cores.Borda,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(14, 12),
            Margin = new Thickness(0, 4, 0, 0),
            Content = linhas
        };
    }
}
