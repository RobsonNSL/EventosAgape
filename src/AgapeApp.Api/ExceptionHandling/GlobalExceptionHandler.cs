using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.ExceptionHandling;

/// <summary>
/// Converte exceções não tratadas em respostas ProblemDetails padronizadas (em português).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception excecao,
        CancellationToken cancellationToken)
    {
        var (situacao, titulo, detalhe) = Mapear(excecao);

        if (situacao == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                excecao,
                "Erro interno ao processar {Metodo} {Caminho}",
                context.Request.Method,
                context.Request.Path);
        }

        var problema = new ProblemDetails
        {
            Status = situacao,
            Title = titulo,
            Detail = detalhe,
            Instance = context.Request.Path
        };

        problema.Extensions["traceId"] = context.TraceIdentifier;

        if (excecao is ValidacaoException excecaoValidacao)
        {
            problema.Extensions["erros"] = excecaoValidacao.Erros;
        }

        context.Response.StatusCode = situacao;
        await context.Response.WriteAsJsonAsync(problema, cancellationToken);

        return true;
    }

    private static (int Situacao, string Titulo, string Detalhe) Mapear(Exception excecao)
        => excecao switch
        {
            ValidacaoException => (StatusCodes.Status400BadRequest, MensagensErro.TituloDadosInvalidos, excecao.Message),
            RegraNegocioException => (StatusCodes.Status400BadRequest, MensagensErro.TituloRegraNegocioViolada, excecao.Message),
            AutenticacaoException => (StatusCodes.Status401Unauthorized, MensagensErro.TituloNaoAutenticado, excecao.Message),
            NaoAutorizadoException => (StatusCodes.Status403Forbidden, MensagensErro.TituloAcessoNegado, excecao.Message),
            NaoEncontradoException => (StatusCodes.Status404NotFound, MensagensErro.TituloNaoEncontrado, excecao.Message),
            ConflitoException => (StatusCodes.Status409Conflict, MensagensErro.TituloConflito, excecao.Message),
            _ => (StatusCodes.Status500InternalServerError, MensagensErro.TituloErroInterno,
                MensagensErro.ErroInesperado)
        };
}
