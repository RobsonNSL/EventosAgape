using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Vínculo de um produto ao evento (administrador): preço praticado no evento,
/// se controla estoque e a quantidade inicial.
/// </summary>
public partial class EventoProdutoFormViewModel : BaseViewModel
{
    private Guid _eventoId;
    private Guid _eventoProdutoId;
    private Guid _produtoId;

    public EventoProdutoFormViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitulo))]
    private string produtoNome = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitulo))]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    private string preco = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarQuantidade))]
    private bool controlaEstoque = true;

    [ObservableProperty]
    private string quantidade = "0";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Titulo), nameof(Subtitulo), nameof(TextoBotao), nameof(MostrarQuantidade))]
    private bool ehNovo = true;

    public bool MostrarQuantidade => EhNovo && ControlaEstoque;

    public string Titulo => EhNovo ? "Vincular produto" : "Editar no evento";

    public string Subtitulo => $"{ProdutoNome} · {EventoNome}";

    public string TextoBotao => EhNovo ? "Vincular ao evento" : "Salvar alterações";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        _eventoId = Parametros.Guid(parametros, "eventoId");
        EventoNome = Parametros.Texto(parametros, "eventoNome");

        var eventoProduto = Parametros.Objeto<EventoProdutoItem>(parametros, "eventoProduto");

        if (eventoProduto is not null)
        {
            _eventoProdutoId = eventoProduto.Id;
            _produtoId = eventoProduto.ProdutoId;
            ProdutoNome = eventoProduto.ProdutoNome;
            Preco = eventoProduto.Preco.ToString("N2", Formatos.Cultura);
            ControlaEstoque = eventoProduto.ControlaEstoque;
            EhNovo = false;
            return;
        }

        var produto = Parametros.Objeto<ProdutoDTO>(parametros, "produto");

        if (produto is not null)
        {
            _produtoId = produto.Id;
            ProdutoNome = produto.Nome;
            Preco = produto.PrecoPadrao is decimal padrao
                ? padrao.ToString("N2", Formatos.Cultura)
                : string.Empty;
        }

        EhNovo = true;
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (!Formatos.TentarValor(Preco, out var valor) || valor <= 0)
        {
            MensagemErro = "Informe o preço que será cobrado neste evento (ex.: 7,50).";
            return;
        }

        var quantidadeInicial = 0;

        if (EhNovo && ControlaEstoque)
        {
            if (!int.TryParse(Quantidade, out quantidadeInicial) || quantidadeInicial < 0)
            {
                MensagemErro = "Informe a quantidade em estoque (pode ser zero).";
                return;
            }
        }

        await ExecutarAsync(async () =>
        {
            if (EhNovo)
                await Api.PostAsync<EventoProdutoDTO>(
                    $"api/eventos/{_eventoId}/produtos",
                    new
                    {
                        produtoId = _produtoId,
                        preco = valor,
                        controlaEstoque = ControlaEstoque,
                        quantidadeEstoqueInicial = quantidadeInicial
                    });
            else
                await Api.PutAsync<EventoProdutoDTO>(
                    $"api/evento-produtos/{_eventoProdutoId}",
                    new { preco = valor, controlaEstoque = ControlaEstoque });

            await NavegarAsync("..");
        });
    }
}
