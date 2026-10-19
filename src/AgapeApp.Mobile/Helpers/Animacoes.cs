using Microsoft.Maui.Controls.Shapes;

namespace AgapeApp.Mobile.Helpers;

/// <summary>
/// Animações do app: comemoração das operações concluídas, contagem de valores e entrada
/// em cascata dos cards. Todas são "cortesia": se algo falhar, os elementos ficam no
/// estado final e a tela segue funcionando.
/// </summary>
public static class Animacoes
{
    private static readonly Color[] CoresConfete =
    [
        Color.FromArgb("#16A34A"), Color.FromArgb("#2563EB"), Color.FromArgb("#F59E0B"),
        Color.FromArgb("#DC2626"), Color.FromArgb("#8B5CF6"), Color.FromArgb("#06B6D4")
    ];

    /// <summary>
    /// Comemoração de sucesso: o círculo surge com efeito mola, o ✓ salta, uma onda se
    /// expande e confetes se espalham a partir do centro (desenhados em <paramref name="palco"/>).
    /// </summary>
    public static async Task CelebrarAsync(VisualElement circulo, VisualElement icone, View onda, Grid palco)
    {
        try
        {
            circulo.Scale = 0;
            icone.Scale = 0;
            icone.Rotation = -30;
            onda.Scale = 1;
            onda.Opacity = 0;

            Vibrar();

            await circulo.ScaleToAsync(1, 420, Easing.SpringOut);

            _ = OndaAsync(onda);
            _ = ConfeteAsync(palco);

            await Task.WhenAll(
                icone.ScaleToAsync(1.25, 180, Easing.CubicOut),
                icone.RotateToAsync(0, 180, Easing.CubicOut));
            await icone.ScaleToAsync(1, 140, Easing.CubicIn);
        }
        catch (Exception)
        {
            circulo.Scale = 1;
            icone.Scale = 1;
            icone.Rotation = 0;
        }
    }

    /// <summary>Faz o valor "subir" de zero até o número final no rótulo.</summary>
    public static Task ContarAsync(Label rotulo, decimal ate, uint duracao = 700)
    {
        var conclusao = new TaskCompletionSource();

        try
        {
            rotulo.Text = Formatos.Moeda(0);

            new Animation(v => rotulo.Text = Formatos.Moeda(Math.Round(ate * (decimal)v, 2)), 0, 1, Easing.CubicOut)
                .Commit(rotulo, "contagem", 16, duracao, finished: (_, _) =>
                {
                    rotulo.Text = Formatos.Moeda(ate);
                    conclusao.TrySetResult();
                });
        }
        catch (Exception)
        {
            rotulo.Text = Formatos.Moeda(ate);
            conclusao.TrySetResult();
        }

        return conclusao.Task;
    }

    /// <summary>Os filhos do layout aparecem um a um, subindo levemente.</summary>
    public static async Task CascataAsync(Layout conteudo, uint atraso = 60)
    {
        var filhos = conteudo.OfType<VisualElement>().Where(f => f.IsVisible).ToList();

        foreach (var filho in filhos)
        {
            filho.Opacity = 0;
            filho.TranslationY = 18;
        }

        try
        {
            var animacoes = new List<Task>();

            foreach (var filho in filhos)
            {
                animacoes.Add(Task.WhenAll(
                    filho.FadeToAsync(1, 260, Easing.CubicOut),
                    filho.TranslateToAsync(0, 0, 300, Easing.CubicOut)));

                await Task.Delay((int)atraso);
            }

            await Task.WhenAll(animacoes);
        }
        catch (Exception)
        {
            foreach (var filho in filhos)
            {
                filho.Opacity = 1;
                filho.TranslationY = 0;
            }
        }
    }

    /// <summary>"Pulso" curto para chamar a atenção para um valor que mudou.</summary>
    public static async Task PulsarAsync(VisualElement elemento, double escala = 1.12)
    {
        try
        {
            elemento.CancelAnimations();
            await elemento.ScaleToAsync(escala, 90, Easing.CubicOut);
            await elemento.ScaleToAsync(1, 140, Easing.CubicIn);
        }
        catch (Exception)
        {
            elemento.Scale = 1;
        }
    }

    private static async Task OndaAsync(View onda)
    {
        for (var i = 0; i < 2; i++)
        {
            onda.Scale = 1;
            onda.Opacity = 0.55;

            await Task.WhenAll(
                onda.ScaleToAsync(1.9, 700, Easing.CubicOut),
                onda.FadeToAsync(0, 700, Easing.CubicIn));
        }
    }

    private static async Task ConfeteAsync(Grid palco)
    {
        var sorteio = new Random();
        var pecas = new List<(View Peca, double X, double Y)>();

        for (var i = 0; i < 18; i++)
        {
            var angulo = (Math.PI * 2 / 18 * i) + (sorteio.NextDouble() * 0.3);
            var distancia = 90 + (sorteio.NextDouble() * 70);
            var tamanho = 6 + (sorteio.NextDouble() * 6);

            View peca = i % 3 == 0
                ? new Ellipse { WidthRequest = tamanho, HeightRequest = tamanho, Fill = CoresConfete[i % CoresConfete.Length] }
                : new BoxView
                {
                    WidthRequest = tamanho,
                    HeightRequest = tamanho * 0.5,
                    CornerRadius = 1,
                    Color = CoresConfete[i % CoresConfete.Length],
                    Rotation = sorteio.Next(0, 180)
                };

            peca.HorizontalOptions = LayoutOptions.Center;
            peca.VerticalOptions = LayoutOptions.Center;
            peca.InputTransparent = true;

            palco.Add(peca);
            pecas.Add((peca, Math.Cos(angulo) * distancia, Math.Sin(angulo) * distancia));
        }

        try
        {
            await Task.WhenAll(pecas.Select(async p =>
            {
                await Task.WhenAll(
                    p.Peca.TranslateToAsync(p.X, p.Y, 650, Easing.CubicOut),
                    p.Peca.RotateToAsync(p.Peca.Rotation + 220, 650));

                // Depois de abrir, cai um pouco e some.
                await Task.WhenAll(
                    p.Peca.TranslateToAsync(p.X, p.Y + 40, 450, Easing.CubicIn),
                    p.Peca.FadeToAsync(0, 450));
            }));
        }
        catch (Exception)
        {
            // Animação interrompida (saiu da tela): só limpa.
        }
        finally
        {
            foreach (var (peca, _, _) in pecas)
                palco.Remove(peca);
        }
    }

    private static void Vibrar()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        }
        catch (Exception)
        {
            // Aparelho sem vibração.
        }
    }
}
