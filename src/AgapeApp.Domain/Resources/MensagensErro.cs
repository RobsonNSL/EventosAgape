using System.Resources;
using System.Runtime.CompilerServices;

namespace AgapeApp.Domain.Resources;

/// <summary>
/// Acesso tipado às mensagens de erro devolvidas ao cliente (arquivo MensagensErro.resx).
/// Para adicionar uma mensagem: inclua a chave no .resx e uma propriedade com o mesmo nome aqui.
/// Se a chave não existir no .resx, o acessor devolve o próprio nome da propriedade
/// (fica visível na resposta e o teste MensagensErroTests acusa).
/// </summary>
public static class MensagensErro
{
    private static readonly ResourceManager Gerenciador =
        new("AgapeApp.Domain.Resources.MensagensErro", typeof(MensagensErro).Assembly);

    public static string CredenciaisInvalidas => Obter();

    public static string EmailInvalido => Obter();

    public static string EmailObrigatorio => Obter();

    public static string EmailTamanhoMaximo => Obter();

    public static string EmailUsuarioObrigatorio => Obter();

    public static string ErroInesperado => Obter();

    public static string HashSenhaObrigatorio => Obter();

    public static string NomeObrigatorio => Obter();

    public static string NomeTamanhoMaximo => Obter();

    public static string NomeUsuarioObrigatorio => Obter();

    public static string PerfilInvalido => Obter();

    public static string RequisicaoInvalida => Obter();

    public static string SenhaObrigatoria => Obter();

    public static string SenhaTamanhoMinimo => Obter();

    public static string SessaoExpirada => Obter();

    public static string SessaoInvalida => Obter();

    public static string TituloAcessoNegado => Obter();

    public static string TituloConflito => Obter();

    public static string TituloDadosInvalidos => Obter();

    public static string TituloErroInterno => Obter();

    public static string TituloNaoAutenticado => Obter();

    public static string TituloNaoEncontrado => Obter();

    public static string TituloRegraNegocioViolada => Obter();

    public static string TokenRenovacaoHashObrigatorio => Obter();

    public static string TokenRenovacaoObrigatorio => Obter();

    public static string TokenRenovacaoUsuarioObrigatorio => Obter();

    public static string UsuarioDesativado => Obter();

    public static string UsuarioEmailDuplicado => Obter();

    public static string UsuarioNaoEncontrado => Obter();

    public static string ValidacaoFalhou => Obter();

    private static string Obter([CallerMemberName] string chave = "") =>
        Gerenciador.GetString(chave) ?? chave;
}
