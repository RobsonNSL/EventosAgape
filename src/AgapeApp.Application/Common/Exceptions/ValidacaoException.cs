using AgapeApp.Domain.Resources;

namespace AgapeApp.Application.Common.Exceptions;

/// <summary>
/// Lançada quando uma ou mais validações de entrada falham (FluentValidation).
/// </summary>
public class ValidacaoException : Exception
{
    public ValidacaoException(IReadOnlyDictionary<string, string[]> erros)
        : base(MensagensErro.ValidacaoFalhou)
    {
        Erros = erros;
    }

    /// <summary>
    /// Erros agrupados por campo.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Erros { get; }
}
