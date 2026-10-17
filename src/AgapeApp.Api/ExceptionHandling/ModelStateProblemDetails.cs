using AgapeApp.Domain.Resources;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.ExceptionHandling;

/// <summary>
/// Monta a resposta 400 no padrão ProblemDetails em português quando o model binding falha
/// (JSON malformado, tipo incorreto, corpo vazio etc.), no mesmo formato do GlobalExceptionHandler.
/// As mensagens de campo do framework (em inglês) são substituídas pela mensagem do resources.
/// </summary>
public static class ModelStateProblemDetails
{
    public static IActionResult Criar(ActionContext contexto)
    {
        var erros = contexto.ModelState
            .Where(entrada => entrada.Value?.Errors.Count > 0)
            .ToDictionary(
                entrada => entrada.Key,
                entrada => entrada.Value!.Errors
                    .Select(_ => MensagensErro.RequisicaoInvalida)
                    .ToArray());

        var problema = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = MensagensErro.TituloDadosInvalidos,
            Detail = MensagensErro.ValidacaoFalhou,
            Instance = contexto.HttpContext.Request.Path
        };

        problema.Extensions["erros"] = erros;
        problema.Extensions["traceId"] = contexto.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problema)
        {
            ContentTypes = { "application/problem+json" }
        };
    }
}
