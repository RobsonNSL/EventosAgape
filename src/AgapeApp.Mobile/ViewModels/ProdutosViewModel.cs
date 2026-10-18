using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views.Janelas;
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

    public ColecaoObservavel<ProdutoItem> Produtos { get; } = [];

    [ObservableProperty]
    private string busca = string.Empty;

    partial void OnBuscaChanged(string value) => Filtrar();

    public Task IniciarAsync()
        => PrecisaRecarregar() ? ExecutarAsync(CarregarAsync) : Task.CompletedTask;

    [RelayCommand]
    private Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

    [RelayCommand]
    private Task NovoAsync() => NavegarAsync("produto-form");

    [RelayCommand]
    private async Task AcoesAsync(ProdutoItem? produto)
    {
        if (produto is null)
            return;

        var escolha = await EscolherAsync(
            produto.Nome,
            produto.TextoPreco,
            new OpcaoDialogo("editar", "Editar produto", "Nome e preço sugerido", "\uf303"),
            produto.Ativo
                ? new OpcaoDialogo("ativacao", "Desativar produto", "Some das listas de novos vínculos", "\uf05e", TomDialogo.Perigo)
                : new OpcaoDialogo("ativacao", "Reativar produto", "Volta a poder ser vinculado a eventos", "\uf00c", TomDialogo.Sucesso));

        switch (escolha)
        {
            case "editar":
                await NavegarAsync("produto-form", "produto", produto);
                break;

            case "ativacao":
                await AlterarAtivacaoAsync(produto, ativo: !produto.Ativo);
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
        MarcarComoAtualizado();
    }

    private void Filtrar()
    {
        var termo = Busca.Trim();

        Produtos.Substituir(_todos.Where(produto => Formatos.Contem(produto.Nome, termo)));
    }

    private async Task AlterarAtivacaoAsync(ProdutoItem produto, bool ativo)
    {
        if (!ativo && !await ConfirmarAsync(
                "Desativar produto?",
                $"{produto.Nome} deixa de aparecer na lista para vincular a eventos. Dá para reativar depois.",
                "Desativar",
                TomDialogo.Perigo,
                "\uf05e"))
        {
            return;
        }

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync(
                $"api/produtos/{produto.Id}/{(ativo ? "ativar" : "desativar")}",
                null);

            await CarregarAsync();
            Notificar(ativo ? $"{produto.Nome} reativado" : $"{produto.Nome} desativado");
        });
    }
}
