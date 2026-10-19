using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views.Janelas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Catálogo de produtos (administrador): listar, buscar, cadastrar, editar, (des)ativar
/// e definir a imagem de cada produto (galeria ou câmera).
/// </summary>
public partial class ProdutosViewModel : BaseViewModel
{
    private readonly FotoUsuarioService _imagens;

    private IReadOnlyList<ProdutoItem> _todos = [];

    public ProdutosViewModel(ApiClient api, SessaoService sessao, FotoUsuarioService imagens) : base(api, sessao)
    {
        _imagens = imagens;
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

        var opcoes = new List<OpcaoDialogo>
        {
            new("editar", "Editar produto", "Nome e preço sugerido", "\uf303"),
            new("imagem", produto.TemImagem ? "Trocar imagem" : "Colocar imagem", "Foto da galeria ou da câmera", "\uf030", TomDialogo.Sucesso)
        };

        if (produto.TemImagem)
            opcoes.Add(new OpcaoDialogo("remover-imagem", "Remover imagem", "Volta a mostrar o ícone", "\uf2ed", TomDialogo.Neutro));

        opcoes.Add(produto.Ativo
            ? new OpcaoDialogo("ativacao", "Desativar produto", "Some das listas de novos vínculos", "\uf05e", TomDialogo.Perigo)
            : new OpcaoDialogo("ativacao", "Reativar produto", "Volta a poder ser vinculado a eventos", "\uf00c", TomDialogo.Sucesso));

        var escolha = await EscolherAsync(produto.Nome, produto.TextoPreco, [.. opcoes]);

        switch (escolha)
        {
            case "editar":
                await NavegarAsync("produto-form", "produto", produto);
                break;

            case "imagem":
                await EscolherImagemAsync(produto);
                break;

            case "remover-imagem":
                await RemoverImagemAsync(produto);
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

        // A lista aparece na hora com o ícone; as imagens chegam em seguida.
        _ = _imagens.CarregarEmLoteAsync(_todos);
    }

    private void Filtrar()
    {
        var termo = Busca.Trim();

        Produtos.Substituir(_todos.Where(produto => Formatos.Contem(produto.Nome, termo)));
    }

    private async Task EscolherImagemAsync(ProdutoItem produto)
    {
        var origem = MediaPicker.Default.IsCaptureSupported
            ? await EscolherAsync(
                $"Imagem de {produto.Nome}",
                "Aparece no catálogo, nos produtos do evento e na tela de venda.",
                new OpcaoDialogo("galeria", "Escolher da galeria", null, "\uf03e"),
                new OpcaoDialogo("camera", "Tirar foto agora", null, "\uf030", TomDialogo.Sucesso))
            : "galeria";

        if (origem is null)
            return;

        FileResult? arquivo;

        try
        {
            var opcoes = FotoUsuarioService.OpcoesCaptura($"Imagem de {produto.Nome}");
            arquivo = origem == "camera"
                ? await MediaPicker.Default.CapturePhotoAsync(opcoes)
                : (await MediaPicker.Default.PickPhotosAsync(opcoes))?.FirstOrDefault();
        }
        catch (PermissionException)
        {
            MensagemErro = "Permita o acesso às fotos e à câmera para escolher a imagem.";
            return;
        }
        catch (Exception)
        {
            MensagemErro = "Não foi possível abrir as fotos do aparelho.";
            return;
        }

        if (arquivo is null)
            return;

        await ExecutarAsync(async () =>
        {
            await using var conteudo = await arquivo.OpenReadAsync();

            await Api.PostArquivoAsync<ProdutoDTO>(
                $"api/produtos/{produto.Id}/imagem",
                conteudo,
                arquivo.FileName,
                arquivo.ContentType);

            await CarregarAsync();
            Notificar($"Imagem de {produto.Nome} atualizada");
        });
    }

    private async Task RemoverImagemAsync(ProdutoItem produto)
    {
        if (!await ConfirmarAsync(
                "Remover imagem?",
                $"{produto.Nome} volta a aparecer com o ícone no lugar da foto.",
                "Remover",
                TomDialogo.Perigo,
                "\uf2ed"))
        {
            return;
        }

        await ExecutarAsync(async () =>
        {
            await Api.DeleteAsync<ProdutoDTO>($"api/produtos/{produto.Id}/imagem");
            await CarregarAsync();
            Notificar("Imagem removida");
        });
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
