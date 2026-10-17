using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Perfil do usuário logado: dados, foto, biometria, troca de senha e saída.</summary>
public partial class PerfilViewModel : BaseViewModel
{
    private readonly BiometriaService _biometria;
    private readonly FotoUsuarioService _fotos;

    public PerfilViewModel(
        ApiClient api,
        SessaoService sessao,
        BiometriaService biometria,
        FotoUsuarioService fotos) : base(api, sessao)
    {
        _biometria = biometria;
        _fotos = fotos;
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
    [NotifyPropertyChangedFor(nameof(TemFoto), nameof(SemFoto))]
    private ImageSource? foto;

    /// <summary>Há foto de perfil cadastrada (mostra a imagem no lugar das iniciais).</summary>
    public bool TemFoto => Foto is not null;

    public bool SemFoto => Foto is null;

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

    /// <summary>Baixa a foto de perfil (o carregador nativo de imagens não envia token/cabeçalho do ngrok).</summary>
    public async Task CarregarFotoAsync()
        => Foto = await _fotos.CarregarAsync(Sessao.Usuario?.CaminhoFoto);

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

    /// <summary>Escolhe uma imagem no aparelho e envia como foto de perfil.</summary>
    [RelayCommand]
    private async Task AlterarFotoAsync()
    {
        await ExecutarAsync(async () =>
        {
            FileResult? escolhida;

            try
            {
                // PickPhotosAsync substitui o PickPhotoAsync obsoleto; limite 1 = escolha única.
                var escolhidas = await MediaPicker.Default.PickPhotosAsync(
                    new MediaPickerOptions { Title = "Escolha sua foto", SelectionLimit = 1 });

                escolhida = escolhidas?.FirstOrDefault();
            }
            catch (PermissionException)
            {
                MensagemErro = "Permita o acesso às fotos do aparelho para escolher sua imagem.";
                return;
            }

            if (escolhida is null)
                return;

            await using var conteudo = await escolhida.OpenReadAsync();

            var usuario = await Api.PostArquivoAsync<UsuarioDTO>(
                "api/autenticacao/minha-foto",
                conteudo,
                escolhida.FileName,
                escolhida.ContentType);

            await Sessao.AtualizarFotoAsync(usuario.CaminhoFoto);
            await CarregarFotoAsync();
        });
    }

    /// <summary>Remove a foto de perfil (volta a mostrar as iniciais).</summary>
    [RelayCommand]
    private async Task RemoverFotoAsync()
    {
        if (SemFoto)
            return;

        await ExecutarAsync(async () =>
        {
            var usuario = await Api.DeleteAsync<UsuarioDTO>("api/autenticacao/minha-foto");

            await Sessao.AtualizarFotoAsync(usuario.CaminhoFoto);
            await CarregarFotoAsync();
        });
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
