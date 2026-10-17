namespace AgapeApp.Dominio.Excecoes;

/// <summary>
/// Lançada quando uma regra de negócio do domínio é violada.
/// </summary>
public class ExcecaoRegraNegocio : Exception
{
    public ExcecaoRegraNegocio(string mensagem) : base(mensagem)
    {
    }
}
