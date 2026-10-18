using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Cadastro e edição de produto do catálogo (administrador).</summary>
public partial class ProdutoFormViewModel : BaseViewModel
{
    private Guid _id;

    public ProdutoFormViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string precoPadrao = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Titulo), nameof(Subtitulo), nameof(TextoBotao))]
    private bool ehNovo = true;

    public string Titulo => EhNovo ? "Novo produto" : "Editar produto";

    public string Subtitulo => EhNovo
        ? "Ex.: Pastel, Refrigerante, Ficha de bingo"
        : "Altere o nome ou o preço sugerido";

    public string TextoBotao => EhNovo ? "Cadastrar produto" : "Salvar alterações";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        var produto = Parametros.Objeto<ProdutoItem>(parametros, "produto");

        if (produto is null)
        {
            EhNovo = true;
            return;
        }

        _id = produto.Id;
        Nome = produto.Nome;
        PrecoPadrao = produto.PrecoPadrao is decimal preco
            ? preco.ToString("N2", Formatos.Cultura)
            : string.Empty;
        EhNovo = false;
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            MensagemErro = "Informe o nome do produto.";
            return;
        }

        decimal? preco = null;

        if (!string.IsNullOrWhiteSpace(PrecoPadrao))
        {
            if (!Formatos.TentarValor(PrecoPadrao, out var valor) || valor <= 0)
            {
                MensagemErro = "Informe um preço válido (ex.: 7,50) ou deixe o campo vazio.";
                return;
            }

            preco = valor;
        }

        await ExecutarAsync(async () =>
        {
            if (EhNovo)
                await Api.PostAsync<ProdutoDTO>("api/produtos", new { nome = Nome, precoPadrao = preco });
            else
                await Api.PutAsync<ProdutoDTO>($"api/produtos/{_id}", new { nome = Nome, precoPadrao = preco });

            Notificar(EhNovo ? $"{Nome} cadastrado" : "Produto atualizado");
            await NavegarAsync("..");
        });
    }
}
