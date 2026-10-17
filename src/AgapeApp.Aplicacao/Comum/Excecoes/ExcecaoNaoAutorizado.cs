namespace AgapeApp.Aplicacao.Comum.Excecoes;

/// <summary>
/// Lançada quando o usuário autenticado não tem permissão para a operação.
/// </summary>
public class ExcecaoNaoAutorizado : Exception
{
    public ExcecaoNaoAutorizado(string mensagem) : base(mensagem)
    {
    }
}
