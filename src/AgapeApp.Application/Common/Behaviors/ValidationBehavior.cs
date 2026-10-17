using AgapeApp.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace AgapeApp.Application.Common.Behaviors;

/// <summary>
/// Comportamento do MediatR que executa os validators do FluentValidation
/// antes de cada caso de uso. Se houver falhas, lança <see cref="ValidacaoException"/>.
/// </summary>
public class ValidationBehavior<TRequisicao, TResposta> : IPipelineBehavior<TRequisicao, TResposta>
    where TRequisicao : notnull
{
    private readonly IEnumerable<IValidator<TRequisicao>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequisicao>> validators)
    {
        _validators = validators;
    }

    public async Task<TResposta> Handle(
        TRequisicao requisicao,
        RequestHandlerDelegate<TResposta> proximo,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await proximo(cancellationToken);

        var context = new ValidationContext<TRequisicao>(requisicao);

        var resultados = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var falhas = resultados
            .SelectMany(resultado => resultado.Errors)
            .Where(erro => erro is not null)
            .ToList();

        if (falhas.Count == 0)
            return await proximo(cancellationToken);

        var erros = falhas
            .GroupBy(erro => erro.PropertyName)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(erro => erro.ErrorMessage).ToArray());

        throw new ValidacaoException(erros);
    }
}
