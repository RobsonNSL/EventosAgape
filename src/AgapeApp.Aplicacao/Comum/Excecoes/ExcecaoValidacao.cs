namespace AgapeApp.Aplicacao.Comum.Excecoes;

/// <summary>
/// Lançada quando uma ou mais validações de entrada falham (FluentValidation).
/// </summary>
public class ExcecaoValidacao : Exception
{
    public ExcecaoValidacao(IReadOnlyDictionary<string, string[]> erros)
        : base("Um ou mais campos são inválidos.")
    {
        Erros = erros;
    }

    /// <summary>
    /// Erros agrupados por campo.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Erros { get; }
}
