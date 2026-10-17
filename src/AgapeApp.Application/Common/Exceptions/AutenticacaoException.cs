namespace AgapeApp.Application.Common.Exceptions;

/// <summary>
/// Lançada quando a autenticação falha (credenciais inválidas, sessão expirada etc.).
/// </summary>
public class AutenticacaoException : Exception
{
    public AutenticacaoException(string mensagem) : base(mensagem)
    {
    }
}
