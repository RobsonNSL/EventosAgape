using AgapeApp.Mobile.Views.Janelas;

namespace AgapeApp.Mobile.Services;

/// <summary>
/// Ponto único para abrir as janelas do app (confirmação, aviso, opções, campo e seleção).
/// As janelas entram numa fila: uma só fica aberta por vez, o que evita o travamento
/// de abrir uma caixa nativa logo depois de outra no Android.
/// </summary>
public static class Dialogos
{
    private static readonly SemaphoreSlim Fila = new(1, 1);

    private static int _emUso;

    /// <summary>
    /// Há uma janela aberta (ou fechando). Fechar a janela modal dispara o OnAppearing da
    /// tela de trás; as telas consultam isto para não recarregar nada nesse momento.
    /// </summary>
    public static bool Aberto => Volatile.Read(ref _emUso) > 0;

    /// <summary>Pede confirmação; devolve true quando o usuário confirma.</summary>
    public static Task<bool> ConfirmarAsync(
        string titulo,
        string? mensagem,
        string confirmar = "Confirmar",
        string cancelar = "Cancelar",
        TomDialogo tom = TomDialogo.Primario,
        string icone = "\uf059",
        IReadOnlyList<ItemChecagem>? itens = null)
        => MostrarAsync(() => new ConfirmacaoDialogo(titulo, mensagem, confirmar, cancelar, tom, icone, itens), false);

    /// <summary>Aviso com um único botão.</summary>
    public static Task AvisarAsync(
        string titulo,
        string? mensagem,
        TomDialogo tom = TomDialogo.Primario,
        string icone = "\uf05a",
        string botao = "Entendi")
        => MostrarAsync(() => new ConfirmacaoDialogo(titulo, mensagem, botao, null, tom, icone), false);

    /// <summary>Lista de ações; devolve a chave da opção escolhida ou nulo.</summary>
    public static Task<string?> EscolherAsync(string titulo, string? subtitulo, params OpcaoDialogo[] opcoes)
        => MostrarAsync<string?>(() => new OpcoesDialogo(titulo, subtitulo, opcoes), null);

    /// <summary>Campo de texto; devolve o texto confirmado ou nulo.</summary>
    public static Task<string?> PerguntarAsync(string titulo, string? mensagem, OpcoesEntrada? opcoes = null)
        => MostrarAsync<string?>(() => new EntradaDialogo(titulo, mensagem, opcoes ?? new OpcoesEntrada()), null);

    /// <summary>Escolha de um item da lista (o toque já confirma); nulo quando cancelado.</summary>
    public static async Task<T?> SelecionarUmAsync<T>(
        string titulo,
        string? subtitulo,
        IReadOnlyList<T> itens,
        Func<T, ItemSelecao> descrever,
        string? textoVazio = null) where T : class
    {
        var indices = await MostrarAsync<IReadOnlyList<int>?>(
            () => new SelecaoDialogo(titulo, subtitulo, itens.Select(descrever).ToList(), false, null, string.Empty, textoVazio),
            null);

        return indices is [var indice] ? itens[indice] : null;
    }

    /// <summary>Marca vários itens; devolve os marcados (pode ser vazio) ou nulo quando cancelado.</summary>
    public static async Task<IReadOnlyList<T>?> SelecionarVariosAsync<T>(
        string titulo,
        string? subtitulo,
        IReadOnlyList<T> itens,
        Func<T, ItemSelecao> descrever,
        Func<T, bool>? marcado = null,
        string textoConfirmar = "Salvar",
        string? textoVazio = null)
    {
        List<int> iniciais = marcado is null
            ? []
            : Enumerable.Range(0, itens.Count).Where(i => marcado(itens[i])).ToList();

        var indices = await MostrarAsync<IReadOnlyList<int>?>(
            () => new SelecaoDialogo(titulo, subtitulo, itens.Select(descrever).ToList(), true, iniciais, textoConfirmar, textoVazio),
            null);

        return indices?.Select(i => itens[i]).ToList();
    }

    /// <summary>
    /// Câmera em modo contínuo: cada QR lido vai para <paramref name="aoLer"/> e a janela segue
    /// aberta para o próximo. Devolve quantas leituras deram certo.
    /// </summary>
    public static Task<int> LerCartoesAsync(Func<string, Task<ResultadoLeitura>> aoLer)
        => MostrarAsync(() => new LeitorCartoesDialogo(aoLer), 0);

    private static async Task<TResultado> MostrarAsync<TResultado>(
        Func<DialogoBase<TResultado>> criar,
        TResultado padrao)
    {
        await Fila.WaitAsync();
        Interlocked.Increment(ref _emUso);

        try
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var navegacao = Shell.Current?.Navigation
                                ?? Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;

                if (navegacao is null)
                    return padrao;

                var dialogo = criar();
                await navegacao.PushModalAsync(dialogo, false);
                return await dialogo.Resultado;
            });
        }
        catch (Exception)
        {
            return padrao;
        }
        finally
        {
            // O OnAppearing da tela de trás chega logo depois do fechamento; a janela
            // só deixa de contar como aberta depois dele, para a tela não recarregar à toa.
            await Task.Delay(80);
            Interlocked.Decrement(ref _emUso);
            Fila.Release();
        }
    }
}
