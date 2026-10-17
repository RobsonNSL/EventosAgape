using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AgapeApp.Mobile.Config;

namespace AgapeApp.Mobile.Services;

/// <summary>
/// Cliente HTTP da API: anexa o token do usuário e converte respostas de erro
/// em <see cref="ApiException"/> com a mensagem em português vinda do servidor.
/// </summary>
public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly SessaoService _sessao;

    public ApiClient(HttpClient http, SessaoService sessao)
    {
        _http = http;
        _sessao = sessao;
    }

    public Task<T> GetAsync<T>(string rota, CancellationToken ct = default)
        => EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Get, AppConfig.Url(rota)), ct);

    public Task<T> PostAsync<T>(string rota, object corpo, CancellationToken ct = default)
        => EnviarAsync<T>(
            new HttpRequestMessage(HttpMethod.Post, AppConfig.Url(rota)) { Content = ConteudoJson(corpo) },
            ct);

    public Task<T> PutAsync<T>(string rota, object corpo, CancellationToken ct = default)
        => EnviarAsync<T>(
            new HttpRequestMessage(HttpMethod.Put, AppConfig.Url(rota)) { Content = ConteudoJson(corpo) },
            ct);

    public async Task PostSemRespostaAsync(string rota, object? corpo, CancellationToken ct = default)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, AppConfig.Url(rota));
        if (corpo is not null)
            requisicao.Content = ConteudoJson(corpo);

        using var resposta = await ExecutarAsync(requisicao, ct);
        if (!resposta.IsSuccessStatusCode)
            throw await ApiException.DeRespostaAsync(resposta, ct);
    }

    private async Task<T> EnviarAsync<T>(HttpRequestMessage requisicao, CancellationToken ct)
    {
        using (requisicao)
        {
            using var resposta = await ExecutarAsync(requisicao, ct);

            if (!resposta.IsSuccessStatusCode)
                throw await ApiException.DeRespostaAsync(resposta, ct);

            var json = await resposta.Content.ReadAsStringAsync(ct);
            return string.IsNullOrWhiteSpace(json)
                ? default!
                : JsonSerializer.Deserialize<T>(json, Json)!;
        }
    }

    private async Task<HttpResponseMessage> ExecutarAsync(HttpRequestMessage requisicao, CancellationToken ct)
    {
        var token = await _sessao.ObterTokenValidoAsync(ct);
        if (!string.IsNullOrWhiteSpace(token))
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            return await _http.SendAsync(requisicao, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested))
        {
            throw new ApiException(
                HttpStatusCode.ServiceUnavailable,
                "Sem conexão com o servidor. Verifique a rede e tente novamente.");
        }
    }

    private static HttpContent ConteudoJson(object corpo)
        => new StringContent(JsonSerializer.Serialize(corpo, Json), Encoding.UTF8, "application/json");
}
