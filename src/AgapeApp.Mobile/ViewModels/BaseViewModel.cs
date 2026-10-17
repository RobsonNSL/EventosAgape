using System.Net;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Base dos view models: estado de carregamento, mensagem de erro em português
/// e tratamento central das falhas da API (sessão expirada volta para o login).
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
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
    /// Útil nas telas de gestão, que precisam da lista antes de abrir as opções.
    /// </summary>
    protected async Task<T?> ExecutarAsync<T>(Func<Task<T>> operacao)
    {
        T? resultado = default;

        await ExecutarAsync(async () => resultado = await operacao());

        return resultado;
    }

    /// <summary>Mensagem exibida ao usuário; saldo insuficiente ganha o saldo atual.</summary>
    protected virtual string MensagemDeErro(ApiException ex)
        => ex.SaldoAtual is decimal saldo
            ? $"{ex.Message} Saldo atual do cartão: {Formatos.Moeda(saldo)}."
            : ex.Message;

    /// <summary>Mostra um aviso simples (usado pelas telas de gestão).</summary>
    protected static Task AvisarAsync(string titulo, string mensagem)
        => Shell.Current is null
            ? Task.CompletedTask
            : Shell.Current.DisplayAlertAsync(titulo, mensagem, "Ok");

    /// <summary>Pede uma confirmação antes de uma ação destrutiva.</summary>
    protected static Task<bool> ConfirmarAsync(string titulo, string mensagem, string confirmar = "Confirmar")
        => Shell.Current is null
            ? Task.FromResult(false)
            : Shell.Current.DisplayAlertAsync(titulo, mensagem, confirmar, "Cancelar");

    /// <summary>Navega para outra tela, ignorando o toque quando a página já saiu.</summary>
    protected static Task NavegarAsync(string rota)
        => Shell.Current is null ? Task.CompletedTask : Shell.Current.GoToAsync(rota);

    /// <summary>Navega levando um objeto para a próxima tela (formulários de edição).</summary>
    protected static Task NavegarAsync(string rota, string chave, object valor)
        => NavegarAsync(rota, new Dictionary<string, object> { [chave] = valor });

    /// <summary>Navega levando vários parâmetros para a próxima tela.</summary>
    protected static Task NavegarAsync(string rota, IDictionary<string, object> parametros)
        => Shell.Current is null ? Task.CompletedTask : Shell.Current.GoToAsync(rota, parametros);

    /// <summary>Abre a folha de opções da tela; devolve a escolha do usuário.</summary>
    protected static Task<string?> EscolherAsync(string titulo, params string[] opcoes)
        => Shell.Current is null
            ? Task.FromResult<string?>(null)
            : Shell.Current.DisplayActionSheetAsync(titulo, "Cancelar", null, opcoes);

    [RelayCommand]
    private async Task VoltarAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("..");
    }
}
