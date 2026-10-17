using System.Collections.ObjectModel;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Gestão de usuários (administrador): listar, buscar, cadastrar, editar,
/// redefinir a senha e (des)ativar o acesso.
/// </summary>
public partial class UsuariosViewModel : BaseViewModel
{
    private IReadOnlyList<UsuarioItem> _todos = [];

    public UsuariosViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ObservableCollection<UsuarioItem> Usuarios { get; } = [];

    [ObservableProperty]
    private string busca = string.Empty;

    partial void OnBuscaChanged(string value) => Filtrar();

    public Task IniciarAsync() => ExecutarAsync(CarregarAsync);

    [RelayCommand]
    private Task AtualizarAsync() => IniciarAsync();

    [RelayCommand]
    private Task NovoAsync() => NavegarAsync("usuario-form");

    [RelayCommand]
    private async Task AcoesAsync(UsuarioItem? usuario)
    {
        if (usuario is null)
            return;

        var escolha = await EscolherAsync(
            usuario.Nome,
            "Editar dados",
            "Redefinir senha",
            usuario.TextoAtivacao);

        switch (escolha)
        {
            case "Editar dados":
                await NavegarAsync("usuario-form", "usuario", usuario);
                break;

            case "Redefinir senha":
                await RedefinirSenhaAsync(usuario);
                break;

            case "Desativar acesso":
                await AlterarAtivacaoAsync(usuario, ativar: false);
                break;

            case "Ativar acesso":
                await AlterarAtivacaoAsync(usuario, ativar: true);
                break;
        }
    }

    private async Task CarregarAsync()
    {
        var usuarios = await Api.GetAsync<List<UsuarioDTO>>("api/usuarios");

        _todos = usuarios
            .Select(UsuarioItem.De)
            .OrderBy(usuario => usuario.Nome, StringComparer.CurrentCulture)
            .ToList();

        Filtrar();
    }

    private void Filtrar()
    {
        var termo = Busca.Trim();

        var filtrados = _todos
            .Where(usuario => Formatos.Contem(usuario.Nome, termo) || Formatos.Contem(usuario.Email, termo))
            .ToList();

        Usuarios.Clear();

        foreach (var usuario in filtrados)
            Usuarios.Add(usuario);
    }

    private async Task RedefinirSenhaAsync(UsuarioItem usuario)
    {
        if (Shell.Current is null)
            return;

        var novaSenha = await Shell.Current.DisplayPromptAsync(
            "Redefinir senha",
            $"Nova senha de {usuario.Nome} (mínimo 8 caracteres). Avise a pessoa em seguida.",
            "Redefinir",
            "Cancelar",
            "Nova senha",
            maxLength: 60);

        if (string.IsNullOrWhiteSpace(novaSenha))
            return;

        if (novaSenha.Length < 8)
        {
            MensagemErro = "A nova senha precisa ter pelo menos 8 caracteres.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync($"api/usuarios/{usuario.Id}/redefinir-senha", new { novaSenha });

            await AvisarAsync(
                "Senha redefinida",
                $"A senha de {usuario.Nome} foi trocada. Os aparelhos dela vão precisar entrar de novo.");
        });
    }

    private async Task AlterarAtivacaoAsync(UsuarioItem usuario, bool ativar)
    {
        var confirmado = await ConfirmarAsync(
            ativar ? "Ativar acesso" : "Desativar acesso",
            ativar
                ? $"{usuario.Nome} volta a entrar no app com o acesso atual."
                : $"{usuario.Nome} não conseguirá mais entrar no app até ser reativado.",
            ativar ? "Ativar" : "Desativar");

        if (!confirmado)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync(
                $"api/usuarios/{usuario.Id}/{(ativar ? "ativar" : "desativar")}",
                null);

            await CarregarAsync();
        });
    }
}
