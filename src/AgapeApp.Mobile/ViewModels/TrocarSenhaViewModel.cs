using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Troca da própria senha (exige a senha atual).</summary>
public partial class TrocarSenhaViewModel : BaseViewModel
{
    public TrocarSenhaViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    [ObservableProperty]
    private string senhaAtual = string.Empty;

    [ObservableProperty]
    private string novaSenha = string.Empty;

    [ObservableProperty]
    private string confirmacaoNovaSenha = string.Empty;

    public event Action? SenhaAlterada;

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(SenhaAtual) || string.IsNullOrWhiteSpace(NovaSenha))
        {
            MensagemErro = "Preencha todos os campos.";
            return;
        }

        if (NovaSenha.Length < 8)
        {
            MensagemErro = "A nova senha deve ter pelo menos 8 caracteres.";
            return;
        }

        if (NovaSenha != ConfirmacaoNovaSenha)
        {
            MensagemErro = "A confirmação não confere com a nova senha.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync(
                "api/autenticacao/alterar-senha",
                new AlterarSenhaRequest(SenhaAtual, NovaSenha));

            SenhaAtual = string.Empty;
            NovaSenha = string.Empty;
            ConfirmacaoNovaSenha = string.Empty;

            SenhaAlterada?.Invoke();
        });
    }
}
