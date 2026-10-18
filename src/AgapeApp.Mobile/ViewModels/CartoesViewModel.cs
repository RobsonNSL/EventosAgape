using System.Globalization;
using System.Text.RegularExpressions;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views.Janelas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Cartões (administrador): listar, buscar, cadastrar pelo código e ativar/desativar.
/// O cadastro é em sequência: depois de cada cartão o app já sugere o próximo código
/// (CARD-0001 → CARD-0002), para cadastrar um lote sem redigitar tudo.
/// </summary>
public partial class CartoesViewModel : BaseViewModel
{
    private IReadOnlyList<CartaoItem> _todos = [];

    public CartoesViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ColecaoObservavel<CartaoItem> Cartoes { get; } = [];

    [ObservableProperty]
    private string busca = string.Empty;

    [ObservableProperty]
    private string resumo = string.Empty;

    partial void OnBuscaChanged(string value) => Filtrar();

    public Task IniciarAsync()
        => PrecisaRecarregar() ? ExecutarAsync(CarregarAsync) : Task.CompletedTask;

    [RelayCommand]
    private Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

    [RelayCommand]
    private async Task NovoAsync()
    {
        var escolha = await EscolherAsync(
            "Cadastrar cartões",
            "Cada cartão precisa estar cadastrado antes da primeira recarga.",
            new OpcaoDialogo("qr", "Ler o QR dos cartões", "Pela câmera, um atrás do outro — mais rápido para um lote", "\uf029", TomDialogo.Sucesso),
            new OpcaoDialogo("digitar", "Digitar o código", "O app sugere o próximo código em sequência", "\uf11c"));

        var cadastrados = escolha switch
        {
            "qr" => await CadastrarPeloQrAsync(),
            "digitar" => await CadastrarDigitandoAsync(),
            _ => 0
        };

        // A lista só é refeita no fim: mexer nela entre uma janela e outra
        // (com a tela de trás se redesenhando) é justamente o que evitamos.
        if (cadastrados > 0)
            await ExecutarAsync(CarregarAsync);
    }

    /// <summary>Câmera contínua: cada QR lido é cadastrado na hora (duplicados são avisados).</summary>
    private async Task<int> CadastrarPeloQrAsync()
    {
        var conhecidos = CodigosCadastrados();

        return await Dialogos.LerCartoesAsync(async codigo =>
        {
            if (conhecidos.Contains(codigo))
                return new ResultadoLeitura(false, $"{codigo} já estava cadastrado");

            try
            {
                var criado = await Api.PostAsync<CartaoDTO>("api/cartoes", new { codigo });
                conhecidos.Add(criado.Codigo);
                return new ResultadoLeitura(true, $"Cartão {criado.Codigo} cadastrado");
            }
            catch (ApiException ex)
            {
                return new ResultadoLeitura(false, $"{codigo}: {ex.Message}");
            }
        });
    }

    /// <summary>Digitação em sequência, com o próximo código já sugerido.</summary>
    private async Task<int> CadastrarDigitandoAsync()
    {
        var conhecidos = CodigosCadastrados();
        var sugestao = ProximoCodigo(_todos.FirstOrDefault()?.Codigo);
        var cadastrados = 0;

        while (true)
        {
            var codigo = await Dialogos.PerguntarAsync(
                cadastrados == 0 ? "Novo cartão" : "Próximo cartão",
                cadastrados == 0
                    ? "Digite o código impresso no cartão. Depois de salvar, o app já sugere o próximo."
                    : $"{cadastrados} cadastrado(s) agora. Confira o código sugerido ou toque em Cancelar para terminar.",
                new OpcoesEntrada
                {
                    ValorInicial = sugestao,
                    Placeholder = "Ex.: CARD-0001",
                    TamanhoMaximo = 50,
                    TextoConfirmar = "Cadastrar",
                    Icone = "\uf09d",
                    Validar = texto => conhecidos.Contains(CodigoCartao.Normalizar(texto))
                        ? "Esse código já está cadastrado."
                        : null
                });

            if (codigo is null)
                break;

            var criado = await ExecutarAsync(() => Api.PostAsync<CartaoDTO>("api/cartoes", new { codigo = CodigoCartao.Normalizar(codigo) }));

            if (criado is null)
                break;

            cadastrados++;
            conhecidos.Add(criado.Codigo);
            Notificar($"Cartão {criado.Codigo} cadastrado");

            sugestao = ProximoCodigo(criado.Codigo);
        }

        return cadastrados;
    }

    private HashSet<string> CodigosCadastrados()
        => new(_todos.Select(c => c.Codigo), StringComparer.OrdinalIgnoreCase);

    [RelayCommand]
    private async Task AcoesAsync(CartaoItem? cartao)
    {
        if (cartao is null)
            return;

        var escolha = await EscolherAsync(
            $"Cartão {cartao.Codigo}",
            cartao.CriadoEmTexto,
            cartao.Ativo
                ? new OpcaoDialogo("ativacao", "Bloquear cartão", "Perdido ou danificado: recargas e vendas são recusadas", "\uf05e", TomDialogo.Perigo)
                : new OpcaoDialogo("ativacao", "Desbloquear cartão", "Volta a aceitar recargas e vendas", "\uf00c", TomDialogo.Sucesso));

        if (escolha != "ativacao")
            return;

        var ativar = !cartao.Ativo;

        if (!ativar && !await ConfirmarAsync(
                "Bloquear cartão?",
                $"O cartão {cartao.Codigo} deixa de aceitar recargas e vendas em todos os eventos até ser desbloqueado.",
                "Bloquear",
                TomDialogo.Perigo,
                "\uf05e"))
        {
            return;
        }

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync(
                $"api/cartoes/{cartao.Id}/{(ativar ? "ativar" : "desativar")}",
                null);

            await CarregarAsync();
            Notificar(ativar ? $"Cartão {cartao.Codigo} desbloqueado" : $"Cartão {cartao.Codigo} bloqueado");
        });
    }

    private async Task CarregarAsync()
    {
        var cartoes = await Api.GetAsync<List<CartaoDTO>>("api/cartoes?incluirInativos=true");

        _todos = cartoes
            .OrderByDescending(cartao => cartao.CriadoEm)
            .ThenBy(cartao => cartao.Codigo, StringComparer.OrdinalIgnoreCase)
            .Select(CartaoItem.De)
            .ToList();

        Filtrar();
        MarcarComoAtualizado();
    }

    private void Filtrar()
    {
        var termo = Busca.Trim();

        Cartoes.Substituir(_todos.Where(cartao => Formatos.Contem(cartao.Codigo, termo)));

        var bloqueados = _todos.Count(c => !c.Ativo);
        Resumo = bloqueados == 0
            ? $"{_todos.Count} cartões cadastrados"
            : $"{_todos.Count} cartões · {bloqueados} bloqueados";
    }

    /// <summary>Sugere o código seguinte mantendo prefixo e zeros (CARD-0009 → CARD-0010).</summary>
    private static string? ProximoCodigo(string? ultimo)
    {
        if (string.IsNullOrWhiteSpace(ultimo))
            return null;

        var partes = Regex.Match(ultimo, @"^(.*?)(\d+)$");
        if (!partes.Success)
            return null;

        var numero = partes.Groups[2].Value;
        if (!long.TryParse(numero, NumberStyles.None, CultureInfo.InvariantCulture, out var valor))
            return null;

        var proximo = (valor + 1).ToString(CultureInfo.InvariantCulture).PadLeft(numero.Length, '0');
        return partes.Groups[1].Value + proximo;
    }
}
