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

    /// <summary>Mensagem exibida ao usuário; saldo insuficiente ganha o saldo atual.</summary>
    protected virtual string MensagemDeErro(ApiException ex)
        => ex.SaldoAtual is decimal saldo
            ? $"{ex.Message} Saldo atual do cartão: {Formatos.Moeda(saldo)}."
            : ex.Message;

    [RelayCommand]
    private async Task VoltarAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("..");
    }
}
