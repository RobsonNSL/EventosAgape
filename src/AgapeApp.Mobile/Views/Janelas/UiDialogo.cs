using AgapeApp.Mobile.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace AgapeApp.Mobile.Views.Janelas;

/// <summary>Cor de destaque da janela: define o ícone e o botão principal.</summary>
public enum TomDialogo
{
    Primario,
    Sucesso,
    Destaque,
    Perigo,
    Neutro
}

/// <summary>Opção de uma lista de ações (ex.: "Editar", "Desativar").</summary>
public sealed record OpcaoDialogo(
    string Chave,
    string Titulo,
    string? Descricao = null,
    string Icone = "\uf054",
    TomDialogo Tom = TomDialogo.Primario);

/// <summary>Linha de uma lista de seleção (pessoa, produto, setor...).</summary>
public sealed record ItemSelecao(
    string Titulo,
    string? Detalhe = null,
    string? Iniciais = null,
    string Icone = "\uf111");

/// <summary>Item de verificação exibido numa confirmação (ok ou atenção).</summary>
public sealed record ItemChecagem(string Texto, bool Ok);

/// <summary>Peças visuais comuns às janelas (mesmo design system das telas).</summary>
internal static class UiDialogo
{
    public static (Color Fundo, Color Icone, Color Botao, Color TextoBotao) Paleta(TomDialogo tom) => tom switch
    {
        TomDialogo.Sucesso => (Cores.VerdeFundo, Cores.Sucesso, Cores.Sucesso, Cores.Superficie),
        TomDialogo.Destaque => (Cores.AmbarFundo, Cores.AmbarTexto, Cores.Destaque, Cores.TextoPrimario),
        TomDialogo.Perigo => (Cores.PerigoFundo, Cores.Perigo, Cores.Perigo, Cores.Superficie),
        TomDialogo.Neutro => (Cores.CinzaFundo, Cores.CinzaTexto, Cores.TextoPrimario, Cores.Superficie),
        _ => (Cores.AzulFundo, Cores.Primaria, Cores.Primaria, Cores.Superficie)
    };

    public static Border Icone(string glifo, TomDialogo tom, double tamanho = 52)
    {
        var (fundo, cor, _, _) = Paleta(tom);

        return new Border
        {
            WidthRequest = tamanho,
            HeightRequest = tamanho,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = tamanho / 3.2 },
            BackgroundColor = fundo,
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = glifo,
                FontFamily = "fa",
                FontSize = tamanho * 0.4,
                TextColor = cor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
    }

    public static Label Titulo(string texto) => new()
    {
        Text = texto,
        FontFamily = "InterBold",
        FontSize = 20,
        TextColor = Cores.TextoPrimario
    };

    public static Label Mensagem(string? texto) => new()
    {
        Text = texto,
        FontFamily = "Inter",
        FontSize = 15,
        LineHeight = 1.25,
        TextColor = Cores.TextoSecundario,
        IsVisible = !string.IsNullOrWhiteSpace(texto)
    };

    public static Button Botao(string texto, TomDialogo tom)
    {
        var (_, _, cor, textoCor) = Paleta(tom);

        return new Button
        {
            Text = texto,
            BackgroundColor = cor,
            TextColor = textoCor
        };
    }

    public static Button BotaoContorno(string texto) => new()
    {
        Text = texto,
        BackgroundColor = Cores.Superficie,
        TextColor = Cores.TextoPrimario,
        BorderColor = Cores.Borda,
        BorderWidth = 1.5
    };

    /// <summary>Aplica um "afundar" rápido ao toque, para a linha responder na hora.</summary>
    public static async Task PulsarAsync(VisualElement elemento)
    {
        try
        {
            await elemento.ScaleToAsync(0.97, 60);
            await elemento.ScaleToAsync(1, 60);
        }
        catch (Exception)
        {
            elemento.Scale = 1;
        }
    }
}
