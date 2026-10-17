namespace AgapeApp.Aplicacao.Comum.Excecoes;

/// <summary>
/// Lançada quando a autenticação falha (credenciais inválidas, sessão expirada etc.).
/// </summary>
public class ExcecaoAutenticacao : Exception
{
    public ExcecaoAutenticacao(string mensagem) : base(mensagem)
    {
    }
}
