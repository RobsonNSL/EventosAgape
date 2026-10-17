using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Cadastro e edição de usuário (administrador).</summary>
public partial class UsuarioFormViewModel : BaseViewModel
{
    private Guid _id;

    public UsuarioFormViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string senha = string.Empty;

    [ObservableProperty]
    private bool administrador;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Titulo), nameof(Subtitulo), nameof(MostrarSenha), nameof(TextoBotao))]
    private bool ehNovo = true;

    public string Titulo => EhNovo ? "Novo usuário" : "Editar usuário";

    public string Subtitulo => EhNovo
        ? "Cadastre um voluntário ou um administrador"
        : "Atualize os dados cadastrais";

    public bool MostrarSenha => EhNovo;

    public string TextoBotao => EhNovo ? "Cadastrar usuário" : "Salvar alterações";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        var usuario = Parametros.Objeto<UsuarioItem>(parametros, "usuario");

        if (usuario is null)
        {
            EhNovo = true;
            return;
        }

        _id = usuario.Id;
        Nome = usuario.Nome;
        Email = usuario.Email;
        Administrador = usuario.EhAdministrador;
        EhNovo = false;
    }

    /// <summary>Chamado pelos chips de perfil da tela.</summary>
    [RelayCommand]
    private void DefinirPerfil(string? perfil)
        => Administrador = perfil == "Administrador";

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email))
        {
            MensagemErro = "Preencha o nome e o e-mail.";
            return;
        }

        if (EhNovo && Senha.Length < 8)
        {
            MensagemErro = "A senha precisa ter pelo menos 8 caracteres.";
            return;
        }

        var perfil = Administrador ? "Administrador" : "Operador";

        await ExecutarAsync(async () =>
        {
            if (EhNovo)
                await Api.PostAsync<UsuarioDTO>(
                    "api/usuarios",
                    new { nome = Nome, email = Email, senha = Senha, perfil });
            else
                await Api.PutAsync<UsuarioDTO>(
                    $"api/usuarios/{_id}",
                    new { nome = Nome, email = Email, perfil });

            await NavegarAsync("..");
        });
    }
}
