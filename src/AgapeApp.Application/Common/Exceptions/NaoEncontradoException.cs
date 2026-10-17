namespace AgapeApp.Application.Common.Exceptions;

/// <summary>
/// Lançada quando um recurso solicitado não existe.
/// </summary>
public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}
