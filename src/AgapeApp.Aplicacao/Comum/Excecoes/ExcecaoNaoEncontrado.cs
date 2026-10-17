namespace AgapeApp.Aplicacao.Comum.Excecoes;

/// <summary>
/// Lançada quando um recurso solicitado não existe.
/// </summary>
public class ExcecaoNaoEncontrado : Exception
{
    public ExcecaoNaoEncontrado(string mensagem) : base(mensagem)
    {
    }
}
