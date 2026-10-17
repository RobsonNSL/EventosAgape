namespace AgapeApp.Aplicacao.Comum.Excecoes;

/// <summary>
/// Lançada quando a operação conflita com o estado atual dos dados
/// (ex.: e-mail já cadastrado).
/// </summary>
public class ExcecaoConflito : Exception
{
    public ExcecaoConflito(string mensagem) : base(mensagem)
    {
    }
}
