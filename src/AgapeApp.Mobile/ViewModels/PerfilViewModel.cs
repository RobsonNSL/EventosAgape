using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Perfil do usuário logado: dados, biometria, troca de senha e saída.</summary>
public partial class PerfilViewModel : BaseViewModel
{
    private readonly BiometriaService _biometria;

    public PerfilViewModel(ApiClient api, SessaoService sessao, BiometriaService biometria) : base(api, sessao)
    {
        _biometria = biometria;
        Carregar();
    }

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FundoPerfil), nameof(CorPerfil))]
    private string perfil = string.Empty;

    [ObservableProperty]
    private string iniciais = string.Empty;

    [ObservableProperty]
    private bool biometriaDisponivel;

    [ObservableProperty]
    private bool biometriaAtiva;

    public Color FundoPerfil => Perfil == "Administrador" ? Cores.AzulFundo : Cores.VerdeFundo;

    public Color CorPerfil => Perfil == "Administrador" ? Cores.AzulTexto : Cores.VerdeTexto;

    public void Carregar()
    {
        var usuario = Sessao.Usuario;
        Nome = usuario?.Nome ?? string.Empty;
        Email = usuario?.Email ?? string.Empty;
        Perfil = usuario?.Perfil ?? string.Empty;
        Iniciais = Formatos.Iniciais(Nome);
    }

    /// <summary>Verifica se o aparelho tem biometria e se o usuário já a ativou.</summary>
    public async Task PrepararBiometriaAsync()
    {
        BiometriaDisponivel = await _biometria.DisponivelAsync();
        BiometriaAtiva = BiometriaDisponivel && _biometria.Ativada;
    }

    partial void OnBiometriaAtivaChanged(bool value)
    {
        if (BiometriaDisponivel)
            _biometria.Ativada = value;
    }

    [RelayCommand]
    private async Task AlterarSenhaAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("trocar-senha");
    }

    [RelayCommand]
    public async Task SairAsync()
    {
        await ExecutarAsync(async () =>
        {
            await Sessao.SairAsync();

            // Sair da conta também desliga a entrada por biometria neste aparelho.
            _biometria.Ativada = false;
            BiometriaAtiva = false;

            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("//login");
        });
    }
}
