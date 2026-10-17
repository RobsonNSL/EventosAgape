using AgapeApp.Aplicacao.Comum.Excecoes;
using FluentValidation;
using MediatR;

namespace AgapeApp.Aplicacao.Comum.Comportamentos;

/// <summary>
/// Comportamento do MediatR que executa os validadores do FluentValidation
/// antes de cada caso de uso. Se houver falhas, lança <see cref="ExcecaoValidacao"/>.
/// </summary>
public class ComportamentoValidacao<TRequisicao, TResposta> : IPipelineBehavior<TRequisicao, TResposta>
    where TRequisicao : notnull
{
    private readonly IEnumerable<IValidator<TRequisicao>> _validadores;

    public ComportamentoValidacao(IEnumerable<IValidator<TRequisicao>> validadores)
    {
        _validadores = validadores;
    }

    public async Task<TResposta> Handle(
        TRequisicao requisicao,
        RequestHandlerDelegate<TResposta> proximo,
        CancellationToken cancellationToken)
    {
        if (!_validadores.Any())
            return await proximo(cancellationToken);

        var contexto = new ValidationContext<TRequisicao>(requisicao);

        var resultados = await Task.WhenAll(
            _validadores.Select(validador => validador.ValidateAsync(contexto, cancellationToken)));

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

        throw new ExcecaoValidacao(erros);
    }
}
