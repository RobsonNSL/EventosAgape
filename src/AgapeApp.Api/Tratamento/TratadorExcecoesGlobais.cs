using AgapeApp.Aplicacao.Comum.Excecoes;
using AgapeApp.Dominio.Excecoes;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Tratamento;

/// <summary>
/// Converte exceções não tratadas em respostas ProblemDetails padronizadas (em português).
/// </summary>
public class TratadorExcecoesGlobais : IExceptionHandler
{
    private readonly ILogger<TratadorExcecoesGlobais> _registrador;

    public TratadorExcecoesGlobais(ILogger<TratadorExcecoesGlobais> registrador)
    {
        _registrador = registrador;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexto,
        Exception excecao,
        CancellationToken cancellationToken)
    {
        var (situacao, titulo, detalhe) = Mapear(excecao);

        if (situacao == StatusCodes.Status500InternalServerError)
        {
            _registrador.LogError(
                excecao,
                "Erro interno ao processar {Metodo} {Caminho}",
                contexto.Request.Method,
                contexto.Request.Path);
        }

        var problema = new ProblemDetails
        {
            Status = situacao,
            Title = titulo,
            Detail = detalhe,
            Instance = contexto.Request.Path
        };

        problema.Extensions["traceId"] = contexto.TraceIdentifier;

        if (excecao is ExcecaoValidacao excecaoValidacao)
        {
            problema.Extensions["erros"] = excecaoValidacao.Erros;
        }

        contexto.Response.StatusCode = situacao;
        await contexto.Response.WriteAsJsonAsync(problema, cancellationToken);

        return true;
    }

    private static (int Situacao, string Titulo, string Detalhe) Mapear(Exception excecao)
        => excecao switch
        {
            ExcecaoValidacao => (StatusCodes.Status400BadRequest, "Dados inválidos.", excecao.Message),
            ExcecaoRegraNegocio => (StatusCodes.Status400BadRequest, "Regra de negócio violada.", excecao.Message),
            ExcecaoAutenticacao => (StatusCodes.Status401Unauthorized, "Não autenticado.", excecao.Message),
            ExcecaoNaoAutorizado => (StatusCodes.Status403Forbidden, "Acesso negado.", excecao.Message),
            ExcecaoNaoEncontrado => (StatusCodes.Status404NotFound, "Não encontrado.", excecao.Message),
            ExcecaoConflito => (StatusCodes.Status409Conflict, "Conflito.", excecao.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno.",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.")
        };
}
