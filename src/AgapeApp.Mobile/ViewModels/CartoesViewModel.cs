using System.Collections.ObjectModel;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Cartões do evento (administrador): listar, buscar, cadastrar pelo código
/// e ativar/desativar. O saldo continua sendo consultado no terminal.
/// </summary>
public partial class CartoesViewModel : BaseViewModel
{
    private IReadOnlyList<CartaoItem> _todos = [];

    public CartoesViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ObservableCollection<CartaoItem> Cartoes { get; } = [];

    [ObservableProperty]
    private string busca = string.Empty;

    partial void OnBuscaChanged(string value) => Filtrar();

    public Task IniciarAsync() => ExecutarAsync(CarregarAsync);

    [RelayCommand]
    private Task AtualizarAsync() => IniciarAsync();

    [RelayCommand]
    private async Task NovoAsync()
    {
        if (Shell.Current is null)
            return;

        var codigo = await Shell.Current.DisplayPromptAsync(
            "Novo cartão",
            "Digite o código impresso no cartão (ou o número da etiqueta).",
            "Cadastrar",
            "Cancelar",
            "Código do cartão",
            maxLength: 50);

        if (string.IsNullOrWhiteSpace(codigo))
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostAsync<CartaoDTO>("api/cartoes", new { codigo });
            await CarregarAsync();
        });
    }

    [RelayCommand]
    private async Task AcoesAsync(CartaoItem? cartao)
    {
        if (cartao is null)
            return;

        var escolha = await EscolherAsync($"Cartão {cartao.Codigo}", cartao.TextoAtivacao);

        if (escolha != "Ativar cartão" && escolha != "Desativar cartão")
            return;

        var ativar = escolha == "Ativar cartão";

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync(
                $"api/cartoes/{cartao.Id}/{(ativar ? "ativar" : "desativar")}",
                null);

            await CarregarAsync();
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
    }

    private void Filtrar()
    {
        var termo = Busca.Trim();

        var filtrados = _todos.Where(cartao => Formatos.Contem(cartao.Codigo, termo)).ToList();

        Cartoes.Clear();

        foreach (var cartao in filtrados)
            Cartoes.Add(cartao);
    }
}
