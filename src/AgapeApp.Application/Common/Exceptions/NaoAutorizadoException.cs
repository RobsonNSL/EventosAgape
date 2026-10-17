namespace AgapeApp.Application.Common.Exceptions;

/// <summary>
/// Lançada quando o usuário autenticado não tem permissão para a operação.
/// </summary>
public class NaoAutorizadoException : Exception
{
    public NaoAutorizadoException(string mensagem) : base(mensagem)
    {
    }
}
