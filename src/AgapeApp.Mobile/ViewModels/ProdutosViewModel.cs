using System.Collections.ObjectModel;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Catálogo de produtos (administrador): listar, buscar, cadastrar, editar e (des)ativar.</summary>
public partial class ProdutosViewModel : BaseViewModel
{
    private IReadOnlyList<ProdutoItem> _todos = [];

    public ProdutosViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ObservableCollection<ProdutoItem> Produtos { get; } = [];

    [ObservableProperty]
    private string busca = string.Empty;

    partial void OnBuscaChanged(string value) => Filtrar();

    public Task IniciarAsync() => ExecutarAsync(CarregarAsync);

    [RelayCommand]
    private Task AtualizarAsync() => IniciarAsync();

    [RelayCommand]
    private Task NovoAsync() => NavegarAsync("produto-form");

    [RelayCommand]
    private async Task AcoesAsync(ProdutoItem? produto)
    {
        if (produto is null)
            return;

        var escolha = await EscolherAsync(produto.Nome, "Editar produto", produto.TextoAtivacao);

        switch (escolha)
        {
            case "Editar produto":
                await NavegarAsync("produto-form", "produto", produto);
                break;

            case "Desativar produto":
                await AlterarAtivacaoAsync(produto, ativo: false);
                break;

            case "Ativar produto":
                await AlterarAtivacaoAsync(produto, ativo: true);
                break;
        }
    }

    private async Task CarregarAsync()
    {
        var produtos = await Api.GetAsync<List<ProdutoDTO>>("api/produtos?incluirInativos=true");

        _todos = produtos
            .Select(ProdutoItem.De)
            .OrderBy(produto => produto.Nome, StringComparer.CurrentCulture)
            .ToList();

        Filtrar();
    }

    private void Filtrar()
    {
        var termo = Busca.Trim();

        var filtrados = _todos.Where(produto => Formatos.Contem(produto.Nome, termo)).ToList();

        Produtos.Clear();

        foreach (var produto in filtrados)
            Produtos.Add(produto);
    }

    private async Task AlterarAtivacaoAsync(ProdutoItem produto, bool ativo)
    {
        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync(
                $"api/produtos/{produto.Id}/{(ativo ? "ativar" : "desativar")}",
                null);

            await CarregarAsync();
        });
    }
}
