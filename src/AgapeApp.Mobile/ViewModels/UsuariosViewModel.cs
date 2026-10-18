using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views.Janelas;
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

    public ColecaoObservavel<UsuarioItem> Usuarios { get; } = [];

    [ObservableProperty]
    private string busca = string.Empty;

    [ObservableProperty]
    private string resumo = string.Empty;

    partial void OnBuscaChanged(string value) => Filtrar();

    public Task IniciarAsync()
        => PrecisaRecarregar() ? ExecutarAsync(CarregarAsync) : Task.CompletedTask;

    [RelayCommand]
    private Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

    [RelayCommand]
    private Task NovoAsync() => NavegarAsync("usuario-form");

    [RelayCommand]
    private async Task AcoesAsync(UsuarioItem? usuario)
    {
        if (usuario is null)
            return;

        var escolha = await EscolherAsync(
            usuario.Nome,
            $"{usuario.Email} · {usuario.Perfil}",
            new OpcaoDialogo("editar", "Editar dados", "Nome, e-mail e perfil", "\uf303"),
            new OpcaoDialogo("senha", "Redefinir senha", "Cria uma senha nova para a pessoa", "\uf084", TomDialogo.Destaque),
            usuario.Ativo
                ? new OpcaoDialogo("ativacao", "Desativar acesso", "A pessoa não entra mais no app", "\uf05e", TomDialogo.Perigo)
                : new OpcaoDialogo("ativacao", "Reativar acesso", "A pessoa volta a entrar no app", "\uf00c", TomDialogo.Sucesso));

        switch (escolha)
        {
            case "editar":
                await NavegarAsync("usuario-form", "usuario", usuario);
                break;

            case "senha":
                await RedefinirSenhaAsync(usuario);
                break;

            case "ativacao":
                await AlterarAtivacaoAsync(usuario, ativar: !usuario.Ativo);
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

        var administradores = _todos.Count(u => u.EhAdministrador);
        Resumo = $"{_todos.Count - administradores} operadores · {administradores} administradores";

        Filtrar();
        MarcarComoAtualizado();
    }

    private void Filtrar()
    {
        var termo = Busca.Trim();

        Usuarios.Substituir(_todos
            .Where(usuario => Formatos.Contem(usuario.Nome, termo) || Formatos.Contem(usuario.Email, termo)));
    }

    private async Task RedefinirSenhaAsync(UsuarioItem usuario)
    {
        var novaSenha = await Dialogos.PerguntarAsync(
            "Redefinir senha",
            $"Nova senha de {usuario.Nome}. Os aparelhos dela vão precisar entrar de novo.",
            new OpcoesEntrada
            {
                Placeholder = "Mínimo de 8 caracteres",
                Senha = true,
                TamanhoMaximo = 60,
                TextoConfirmar = "Redefinir",
                Icone = "\uf084",
                Tom = TomDialogo.Destaque,
                Validar = texto => texto.Length < 8 ? "A senha precisa ter pelo menos 8 caracteres." : null
            });

        if (novaSenha is null)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync($"api/usuarios/{usuario.Id}/redefinir-senha", new { novaSenha });

            await AvisarAsync(
                "Senha redefinida",
                $"Avise {usuario.Nome} da nova senha. As sessões abertas foram encerradas.",
                TomDialogo.Sucesso,
                "\uf00c");
        });
    }

    private async Task AlterarAtivacaoAsync(UsuarioItem usuario, bool ativar)
    {
        var confirmado = await ConfirmarAsync(
            ativar ? "Reativar acesso?" : "Desativar acesso?",
            ativar
                ? $"{usuario.Nome} volta a entrar no app com a senha atual."
                : $"{usuario.Nome} não conseguirá mais entrar no app até ser reativado. Os aparelhos dela saem da conta.",
            ativar ? "Reativar" : "Desativar",
            ativar ? TomDialogo.Sucesso : TomDialogo.Perigo,
            ativar ? "\uf00c" : "\uf05e");

        if (!confirmado)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync(
                $"api/usuarios/{usuario.Id}/{(ativar ? "ativar" : "desativar")}",
                null);

            await CarregarAsync();
            Notificar(ativar ? $"{usuario.Nome} reativado" : $"{usuario.Nome} desativado");
        });
    }
}
