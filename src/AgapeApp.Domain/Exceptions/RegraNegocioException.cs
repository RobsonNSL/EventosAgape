namespace AgapeApp.Domain.Exceptions;

/// <summary>
/// Lançada quando uma regra de negócio do domínio é violada.
/// </summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem)
    {
    }
}
