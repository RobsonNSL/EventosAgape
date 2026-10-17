namespace AgapeApp.Application.Common.Exceptions;

/// <summary>
/// Lançada quando a operação conflita com o estado atual dos dados
/// (ex.: e-mail já cadastrado).
/// </summary>
public class ConflitoException : Exception
{
    public ConflitoException(string mensagem) : base(mensagem)
    {
    }
}
