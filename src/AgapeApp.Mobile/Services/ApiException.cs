using System.Net;
using System.Text.Json;

namespace AgapeApp.Mobile.Services;

/// <summary>
/// Erro devolvido pela API, já com a mensagem em português (ProblemDetails).
/// Quando a venda falha por saldo insuficiente, traz também o saldo atual
/// para a interface enriquecer a mensagem.
/// </summary>
public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public decimal? SaldoAtual { get; }

    /// <summary>A requisição nem chegou ao servidor (sem rede ou tempo esgotado).</summary>
    public bool SemConexao { get; }

    public ApiException(HttpStatusCode statusCode, string mensagem, decimal? saldoAtual = null)
        : base(mensagem)
    {
        StatusCode = statusCode;
        SaldoAtual = saldoAtual;
        SemConexao = statusCode == HttpStatusCode.ServiceUnavailable && saldoAtual is null;
    }

    /// <summary>Monta a exceção a partir da resposta da API (ProblemDetails).</summary>
    public static async Task<ApiException> DeRespostaAsync(HttpResponseMessage resposta, CancellationToken ct)
    {
        var mensagem = MensagemPadrao(resposta.StatusCode);
        decimal? saldoAtual = null;

        try
        {
            var json = await resposta.Content.ReadAsStringAsync(ct);
            using var documento = JsonDocument.Parse(json);
            var raiz = documento.RootElement;

            if (raiz.TryGetProperty("erros", out var erros) && erros.ValueKind == JsonValueKind.Object)
            {
                // Erros de validação por campo: a primeira mensagem é a mais específica.
                foreach (var campo in erros.EnumerateObject())
                {
                    if (campo.Value.ValueKind == JsonValueKind.Array && campo.Value.GetArrayLength() > 0)
                    {
                        mensagem = campo.Value[0].GetString() ?? mensagem;
                        break;
                    }
                }
            }
            else if (raiz.TryGetProperty("detail", out var detalhe) && !string.IsNullOrWhiteSpace(detalhe.GetString()))
            {
                mensagem = detalhe.GetString()!;
            }
            else if (raiz.TryGetProperty("title", out var titulo) && !string.IsNullOrWhiteSpace(titulo.GetString()))
            {
                mensagem = titulo.GetString()!;
            }

            if (raiz.TryGetProperty("saldoAtual", out var saldo) && saldo.ValueKind == JsonValueKind.Number)
                saldoAtual = saldo.GetDecimal();
        }
        catch (JsonException)
        {
            // Corpo fora do padrão: mantém a mensagem genérica do status.
        }

        return new ApiException(resposta.StatusCode, mensagem, saldoAtual);
    }

    private static string MensagemPadrao(HttpStatusCode codigo) => codigo switch
    {
        HttpStatusCode.Unauthorized => "Sua sessão expirou. Entre novamente.",
        HttpStatusCode.Forbidden => "Seu perfil não tem permissão para esta ação.",
        HttpStatusCode.NotFound => "Registro não encontrado.",
        HttpStatusCode.ServiceUnavailable => "Sem conexão com o servidor. Verifique a rede e tente novamente.",
        _ => "Não foi possível concluir a operação. Tente novamente."
    };
}
