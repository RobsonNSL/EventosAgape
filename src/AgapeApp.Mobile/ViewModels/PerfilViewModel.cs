using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views.Janelas;
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

    public string Versao => $"AgapeApp v{AppInfo.Current.VersionString} · Paróquia Maria Imaculada";

    public void Carregar()
    {
        var usuario = Sessao.Usuario;
        Nome = usuario?.Nome ?? string.Empty;
        Email = usuario?.Email ?? string.Empty;
        Perfil = usuario?.Perfil ?? string.Empty;
        Iniciais = Formatos.Iniciais(Nome);
    }

    /// <summary>Foto de perfil (vem da memória quando já foi baixada nesta sessão).</summary>
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
    private Task AlterarSenhaAsync() => NavegarAsync("trocar-senha");

    /// <summary>Opções da foto: galeria, câmera ou remover.</summary>
    [RelayCommand]
    private async Task AlterarFotoAsync()
    {
        var opcoes = new List<OpcaoDialogo>
        {
            new("galeria", "Escolher da galeria", "Uma foto que já está no aparelho", "\uf03e")
        };

        if (MediaPicker.Default.IsCaptureSupported)
            opcoes.Add(new OpcaoDialogo("camera", "Tirar foto agora", "Abre a câmera do aparelho", "\uf030", TomDialogo.Sucesso));

        if (TemFoto)
            opcoes.Add(new OpcaoDialogo("remover", "Remover foto", "Volta a mostrar as iniciais", "\uf2ed", TomDialogo.Perigo));

        var escolha = await EscolherAsync("Foto de perfil", "Aparece para a equipe nas listas do app.", [.. opcoes]);

        switch (escolha)
        {
            case "galeria":
            case "camera":
                await EnviarFotoAsync(escolha == "camera");
                break;

            case "remover":
                await RemoverFotoAsync();
                break;
        }
    }

    private async Task EnviarFotoAsync(bool camera)
    {
        FileResult? escolhida;

        try
        {
            var opcoes = FotoUsuarioService.OpcoesCaptura("Escolha sua foto");

            escolhida = camera
                ? await MediaPicker.Default.CapturePhotoAsync(opcoes)
                : (await MediaPicker.Default.PickPhotosAsync(opcoes))?.FirstOrDefault();
        }
        catch (PermissionException)
        {
            MensagemErro = camera
                ? "Permita o uso da câmera para tirar a foto."
                : "Permita o acesso às fotos do aparelho para escolher sua imagem.";
            return;
        }
        catch (Exception)
        {
            MensagemErro = "Não foi possível abrir as fotos do aparelho.";
            return;
        }

        if (escolhida is null)
            return;

        await ExecutarAsync(async () =>
        {
            await using var conteudo = await escolhida.OpenReadAsync();

            var usuario = await Api.PostArquivoAsync<UsuarioDTO>(
                "api/autenticacao/minha-foto",
                conteudo,
                escolhida.FileName,
                escolhida.ContentType);

            _fotos.Limpar();
            await Sessao.AtualizarFotoAsync(usuario.CaminhoFoto);
            await CarregarFotoAsync();
            Notificar("Foto atualizada");
        });
    }

    private async Task RemoverFotoAsync()
    {
        if (!await ConfirmarAsync(
                "Remover foto?",
                "As listas do app voltam a mostrar as suas iniciais.",
                "Remover",
                TomDialogo.Perigo,
                "\uf2ed"))
        {
            return;
        }

        await ExecutarAsync(async () =>
        {
            var usuario = await Api.DeleteAsync<UsuarioDTO>("api/autenticacao/minha-foto");

            _fotos.Limpar();
            await Sessao.AtualizarFotoAsync(usuario.CaminhoFoto);
            await CarregarFotoAsync();
            Notificar("Foto removida");
        });
    }

    [RelayCommand]
    private async Task SairAsync()
    {
        var confirmado = await ConfirmarAsync(
            "Sair da conta?",
            BiometriaAtiva
                ? "Para entrar de novo você vai precisar do e-mail e da senha. A entrada por biometria também será desligada neste aparelho."
                : "Para entrar de novo você vai precisar do e-mail e da senha.",
            "Sair",
            TomDialogo.Perigo,
            "\uf2f5");

        if (!confirmado)
            return;

        await ExecutarAsync(async () =>
        {
            await Sessao.SairAsync();

            // Sair da conta também desliga a entrada por biometria neste aparelho.
            _biometria.Ativada = false;
            BiometriaAtiva = false;
            _fotos.Limpar();

            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("//login");
        });
    }
}
