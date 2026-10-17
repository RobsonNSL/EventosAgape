using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private const string ChaveUltimoEmail = "agape.ultimo_email";

    private readonly BiometriaService _biometria;

    public LoginViewModel(ApiClient api, SessaoService sessao, BiometriaService biometria) : base(api, sessao)
    {
        _biometria = biometria;
    }

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string senha = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IconeOlho))]
    private bool senhaEhSecreta = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SemBiometria))]
    private bool biometriaDisponivel;

    /// <summary>Olho aberto/fechado do botão de mostrar senha.</summary>
    public string IconeOlho => SenhaEhSecreta ? "\uf070" : "\uf06e";

    /// <summary>Mostra a dica "a biometria fica disponível após o primeiro login".</summary>
    public bool SemBiometria => !BiometriaDisponivel;

    /// <summary>Atalho de desenvolvimento para apontar o app a outro servidor.</summary>
    public bool MostrarConfiguracaoServidor =>
#if DEBUG
        true;
#else
        false;
#endif

    /// <summary>Preenche o último e-mail e verifica se a biometria está pronta.</summary>
    public async Task PrepararAsync()
    {
        var salvo = Preferences.Default.Get(ChaveUltimoEmail, string.Empty);
        if (!string.IsNullOrWhiteSpace(salvo))
            Email = salvo;

        if (!Sessao.Autenticado)
            await Sessao.CarregarAsync();

        BiometriaDisponivel = _biometria.Ativada
                              && Sessao.TemRenovacaoSalva
                              && await _biometria.DisponivelAsync();
    }

    [RelayCommand]
    private void AlternarVisibilidadeSenha() => SenhaEhSecreta = !SenhaEhSecreta;

    [RelayCommand]
    private async Task EntrarAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Informe o e-mail e a senha para entrar.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            await Sessao.EntrarAsync(Email.Trim(), Senha);
            Senha = string.Empty;
            Preferences.Default.Set(ChaveUltimoEmail, Email.Trim());

            // Primeiro login: oferece ligar a biometria para as próximas entradas.
            if (!_biometria.Ativada
                && await _biometria.DisponivelAsync()
                && Shell.Current is not null)
            {
                var ativar = await Shell.Current.DisplayAlertAsync(
                    "Entrar mais rápido",
                    "Usar a biometria deste aparelho para entrar sem digitar a senha?",
                    "Ativar",
                    "Agora não");

                if (ativar)
                    _biometria.Ativada = true;
            }

            BiometriaDisponivel = false;

            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("//principal");
        });
    }

    [RelayCommand]
    private async Task EntrarComBiometriaAsync()
    {
        if (!_biometria.Ativada)
        {
            BiometriaDisponivel = false;
            MensagemErro = "Entre com e-mail e senha para ativar a biometria neste aparelho.";
            return;
        }

        if (!await _biometria.AutenticarAsync("Confirme sua identidade para entrar"))
        {
            MensagemErro = "Biometria não reconhecida. Use a senha ou tente de novo.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            if (!Sessao.TemRenovacaoSalva)
                await Sessao.CarregarAsync();

            if (!await Sessao.RenovarSessaoSalvaAsync())
            {
                _biometria.Ativada = false;
                BiometriaDisponivel = false;
                MensagemErro = "Sua sessão expirou. Entre com e-mail e senha.";
                return;
            }

            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("//principal");
        });
    }
}
