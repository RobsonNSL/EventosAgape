using System.Net;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views.Janelas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Base dos view models: estado de carregamento, mensagem de erro em português,
/// tratamento central das falhas da API (sessão expirada volta para o login) e
/// controle de quando os dados da tela precisam ser buscados de novo.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    /// <summary>Depois deste tempo a tela busca os dados de novo ao reaparecer.</summary>
    private static readonly TimeSpan ValidadePadrao = TimeSpan.FromMinutes(2);

    private long _versaoCarregada = -1;
    private DateTime _carregadoEm = DateTime.MinValue;

    protected BaseViewModel(ApiClient api, SessaoService sessao)
    {
        Api = api;
        Sessao = sessao;
    }

    protected ApiClient Api { get; }

    protected SessaoService Sessao { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NaoEstaCarregando))]
    private bool estaCarregando;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemErro))]
    private string? mensagemErro;

    public bool NaoEstaCarregando => !EstaCarregando;

    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);

    /// <summary>
    /// A tela ainda não carregou, algo foi gravado depois da última carga ou os dados
    /// estão velhos. Evita refazer todas as chamadas a cada OnAppearing — inclusive
    /// quando ele dispara só porque uma janela (diálogo) foi fechada.
    /// </summary>
    protected bool PrecisaRecarregar(TimeSpan? validade = null)
        => !Dialogos.Aberto
           && (_versaoCarregada != ApiClient.VersaoDados
               || DateTime.UtcNow - _carregadoEm > (validade ?? ValidadePadrao));

    /// <summary>Registra que os dados da tela estão em dia.</summary>
    protected void MarcarComoAtualizado()
    {
        _versaoCarregada = ApiClient.VersaoDados;
        _carregadoEm = DateTime.UtcNow;
    }

    /// <summary>Força a próxima aparição da tela a buscar os dados.</summary>
    protected void MarcarComoDesatualizado() => _versaoCarregada = -1;

    /// <summary>Executa a operação tratando erros da API e protegendo contra toque duplo.</summary>
    protected async Task ExecutarAsync(Func<Task> operacao)
    {
        if (EstaCarregando)
            return;

        try
        {
            EstaCarregando = true;
            MensagemErro = null;
            await operacao();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            await Sessao.LimparAsync();
            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("//login");
        }
        catch (ApiException ex)
        {
            MensagemErro = MensagemDeErro(ex);
        }
        catch (Exception)
        {
            MensagemErro = "Algo deu errado. Tente novamente.";
        }
        finally
        {
            EstaCarregando = false;
        }
    }

    /// <summary>
    /// Igual ao anterior, mas devolve o resultado da operação (ou nulo se ela falhar).
    /// Útil quando a lista precisa chegar antes de abrir uma janela de escolha.
    /// </summary>
    protected async Task<T?> ExecutarAsync<T>(Func<Task<T>> operacao)
    {
        T? resultado = default;

        // Corpo em bloco de propósito: "async () => resultado = await operacao()" devolve o
        // valor atribuído, vira Func<Task<T>> e o C# escolhe ESTA mesma sobrecarga — recursão
        // infinita, StackOverflowException e o app fechando (o bug de designar operador).
        Func<Task> semRetorno = async () => { resultado = await operacao(); };
        await ExecutarAsync(semRetorno);

        return resultado;
    }

    /// <summary>Mensagem exibida ao usuário; saldo insuficiente ganha o saldo atual.</summary>
    protected virtual string MensagemDeErro(ApiException ex)
        => ex.SaldoAtual is decimal saldo
            ? $"{ex.Message} Saldo atual do cartão: {Formatos.Moeda(saldo)}."
            : ex.Message;

    /// <summary>Aviso rápido que some sozinho (ex.: "Produto salvo").</summary>
    protected static void Notificar(string mensagem) => Notificacao.Mostrar(mensagem);

    /// <summary>Aviso com botão "Entendi".</summary>
    protected static Task AvisarAsync(
        string titulo,
        string mensagem,
        TomDialogo tom = TomDialogo.Primario,
        string icone = "\uf05a")
        => Dialogos.AvisarAsync(titulo, mensagem, tom, icone);

    /// <summary>Pede uma confirmação antes de uma ação importante.</summary>
    protected static Task<bool> ConfirmarAsync(
        string titulo,
        string mensagem,
        string confirmar = "Confirmar",
        TomDialogo tom = TomDialogo.Primario,
        string icone = "\uf059",
        IReadOnlyList<ItemChecagem>? itens = null)
        => Dialogos.ConfirmarAsync(titulo, mensagem, confirmar, "Cancelar", tom, icone, itens);

    /// <summary>Abre a lista de ações da tela; devolve a chave escolhida.</summary>
    protected static Task<string?> EscolherAsync(string titulo, string? subtitulo, params OpcaoDialogo[] opcoes)
        => Dialogos.EscolherAsync(titulo, subtitulo, opcoes);

    /// <summary>Navega para outra tela, ignorando o toque quando a página já saiu.</summary>
    protected static Task NavegarAsync(string rota)
        => Shell.Current is null ? Task.CompletedTask : Shell.Current.GoToAsync(rota);

    /// <summary>Navega levando um objeto para a próxima tela (formulários de edição).</summary>
    protected static Task NavegarAsync(string rota, string chave, object valor)
        => NavegarAsync(rota, new Dictionary<string, object> { [chave] = valor });

    /// <summary>Navega levando vários parâmetros para a próxima tela.</summary>
    protected static Task NavegarAsync(string rota, IDictionary<string, object> parametros)
        => Shell.Current is null ? Task.CompletedTask : Shell.Current.GoToAsync(rota, parametros);

    [RelayCommand]
    private async Task VoltarAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("..");
    }
}
