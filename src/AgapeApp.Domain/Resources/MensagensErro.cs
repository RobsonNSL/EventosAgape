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

    public static string AjusteEstoqueInvalido => Obter();

    public static string AjusteEstoqueNegativo => Obter();

    public static string CartaoCodigoDuplicado => Obter();

    public static string CartaoCodigoObrigatorio => Obter();

    public static string CartaoCodigoTamanhoMaximo => Obter();

    public static string CartaoDesativado => Obter();

    public static string CartaoNaoEncontrado => Obter();

    public static string CredenciaisInvalidas => Obter();

    public static string DataFimAnteriorInicio => Obter();

    public static string DataInicioObrigatoria => Obter();

    public static string DesignacaoNaoEncontrada => Obter();

    public static string DispositivoTamanhoMaximo => Obter();

    public static string EmailInvalido => Obter();

    public static string EmailObrigatorio => Obter();

    public static string EmailTamanhoMaximo => Obter();

    public static string EmailUsuarioObrigatorio => Obter();

    public static string ErroInesperado => Obter();

    public static string EstoqueInsuficiente => Obter();

    public static string EstoqueNegativo => Obter();

    public static string EventoApenasAbertoPodeEncerrar => Obter();

    public static string EventoApenasRascunhoPodeAbrir => Obter();

    public static string EventoNaoEncontrado => Obter();

    public static string EventoNaoEstaAberto => Obter();

    public static string EventoNaoPodeSerAlterado => Obter();

    public static string EventoProdutoNaoEncontrado => Obter();

    public static string EventoProdutoSemControleEstoque => Obter();

    public static string HashSenhaObrigatorio => Obter();

    public static string IdObrigatorio => Obter();

    public static string IdOperacaoObrigatoria => Obter();

    public static string NaoPodeDesativarProprioUsuario => Obter();

    public static string NaoPodeRemoverUltimoAdministrador => Obter();

    public static string NomeObrigatorio => Obter();

    public static string NomeTamanhoMaximo => Obter();

    public static string NomeUsuarioObrigatorio => Obter();

    public static string ObservacaoTamanhoMaximo => Obter();

    public static string OperadorDeOutroSetor => Obter();

    public static string OperadorJaDesignado => Obter();

    public static string OperadorSemPermissaoNoEvento => Obter();

    public static string PerfilInvalido => Obter();

    public static string PrecoInvalido => Obter();

    public static string ProdutoDeOutroEvento => Obter();

    public static string ProdutoForaDoSetor => Obter();

    public static string ProdutoJaVinculadoAoEvento => Obter();

    public static string ProdutoJaVinculadoAoSetor => Obter();

    public static string ProdutoNaoEncontrado => Obter();

    public static string QuantidadeEstoqueNegativa => Obter();

    public static string QuantidadeInvalida => Obter();

    public static string RequisicaoInvalida => Obter();

    public static string SaldoInsuficiente => Obter();

    public static string SemPermissao => Obter();

    public static string SenhaAtualIncorreta => Obter();

    public static string SenhaObrigatoria => Obter();

    public static string SenhaTamanhoMinimo => Obter();

    public static string SessaoExpirada => Obter();

    public static string SessaoInvalida => Obter();

    public static string SetorDeOutroEvento => Obter();

    public static string SetorJaCadastrado => Obter();

    public static string SetorNaoEncontrado => Obter();

    public static string TituloAcessoNegado => Obter();

    public static string TituloConflito => Obter();

    public static string TituloDadosInvalidos => Obter();

    public static string TituloErroInterno => Obter();

    public static string TituloNaoAutenticado => Obter();

    public static string TituloNaoEncontrado => Obter();

    public static string TituloRegraNegocioViolada => Obter();

    public static string TokenExpirado => Obter();

    public static string TokenInvalido => Obter();

    public static string TokenNaoInformado => Obter();

    public static string TokenRenovacaoHashObrigatorio => Obter();

    public static string TokenRenovacaoObrigatorio => Obter();

    public static string TokenRenovacaoUsuarioObrigatorio => Obter();

    public static string UsuarioDesativado => Obter();

    public static string UsuarioEmailDuplicado => Obter();

    public static string UsuarioNaoEhOperador => Obter();

    public static string UsuarioNaoEncontrado => Obter();

    public static string ValidacaoFalhou => Obter();

    public static string ValorInvalido => Obter();

    public static string VendaNaoEncontrada => Obter();

    public static string VendaSemItens => Obter();

    public static string VinculoNaoEncontrado => Obter();

    private static string Obter([CallerMemberName] string chave = "") =>
        Gerenciador.GetString(chave) ?? chave;
}
