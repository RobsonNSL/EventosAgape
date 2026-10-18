using AgapeApp.Mobile.Views;

namespace AgapeApp.Mobile.Helpers;

/// <summary>
/// Navegação do terminal do operador. "Próximo cliente" volta para a leitura que já
/// está aberta em vez de empilhar telas novas: antes, cada venda deixava leitura +
/// venda + confirmação na pilha, e depois de algumas dezenas de clientes o app
/// ficava pesado (a memória só crescia).
/// </summary>
public static class Navegacao
{
    /// <summary>Volta para a leitura de cartão da pilha, já pronta para a operação informada.</summary>
    public static async Task VoltarParaLeituraAsync(string origem, string rotaAlternativa)
    {
        var shell = Shell.Current;
        if (shell is null)
            return;

        var pilha = shell.Navigation.NavigationStack;

        for (var i = pilha.Count - 1; i >= 0; i--)
        {
            if (pilha[i] is not LeituraQrPage leitura)
                continue;

            leitura.PrepararNovaLeitura(origem);

            var voltas = pilha.Count - 1 - i;
            if (voltas > 0)
                await shell.GoToAsync(string.Join("/", Enumerable.Repeat("..", voltas)));

            return;
        }

        // Sem leitura na pilha (fluxo incomum): abre uma nova.
        await shell.GoToAsync(rotaAlternativa);
    }

    /// <summary>Volta para o terminal do evento (tela de operação) sem passar pela lista de eventos.</summary>
    public static async Task VoltarParaOperacaoAsync()
    {
        var shell = Shell.Current;
        if (shell is null)
            return;

        var pilha = shell.Navigation.NavigationStack;

        for (var i = pilha.Count - 1; i >= 0; i--)
        {
            if (pilha[i] is not OperacaoEventoPage)
                continue;

            var voltas = pilha.Count - 1 - i;
            if (voltas > 0)
                await shell.GoToAsync(string.Join("/", Enumerable.Repeat("..", voltas)));

            return;
        }

        await shell.GoToAsync("//principal/eventos");
    }
}
