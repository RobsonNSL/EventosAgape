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
    private static readonly TimeSpan EsperaEntreTentativas = TimeSpan.FromMilliseconds(700);

    private static long _versaoDados;

    private readonly HttpClient _http;
    private readonly SessaoService _sessao;

    public ApiClient(HttpClient http, SessaoService sessao)
    {
        _http = http;
        _sessao = sessao;
    }

    /// <summary>
    /// Aumenta a cada gravação bem-sucedida (POST/PUT/DELETE). As telas comparam com a
    /// versão que carregaram para só buscar de novo quando algo mudou.
    /// </summary>
    public static long VersaoDados => Interlocked.Read(ref _versaoDados);

    /// <summary>Leitura: tenta de novo uma vez se a rede falhar (GET não altera nada).</summary>
    public Task<T> GetAsync<T>(string rota, CancellationToken ct = default)
        => EnviarAsync<T>(() => new HttpRequestMessage(HttpMethod.Get, AppConfig.Url(rota)), tentativas: 2, ct);

    public Task<T> PostAsync<T>(string rota, object corpo, CancellationToken ct = default)
        => EnviarAsync<T>(
            () => new HttpRequestMessage(HttpMethod.Post, AppConfig.Url(rota)) { Content = ConteudoJson(corpo) },
            tentativas: 1,
            ct);

    /// <summary>
    /// POST de operação idempotente (recarga e venda carregam o idOperacaoCliente):
    /// com rede instável reenvia sozinho — a API nunca duplica a operação.
    /// </summary>
    public Task<T> PostIdempotenteAsync<T>(string rota, object corpo, CancellationToken ct = default)
        => EnviarAsync<T>(
            () => new HttpRequestMessage(HttpMethod.Post, AppConfig.Url(rota)) { Content = ConteudoJson(corpo) },
            tentativas: 3,
            ct);

    public Task<T> PutAsync<T>(string rota, object corpo, CancellationToken ct = default)
        => EnviarAsync<T>(
            () => new HttpRequestMessage(HttpMethod.Put, AppConfig.Url(rota)) { Content = ConteudoJson(corpo) },
            tentativas: 1,
            ct);

    public async Task PostSemRespostaAsync(string rota, object? corpo, CancellationToken ct = default)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, AppConfig.Url(rota));
        if (corpo is not null)
            requisicao.Content = ConteudoJson(corpo);

        using var resposta = await ExecutarAsync(requisicao, ct);
        if (!resposta.IsSuccessStatusCode)
            throw await ApiException.DeRespostaAsync(resposta, ct);

        Interlocked.Increment(ref _versaoDados);
    }

    /// <summary>Envia um arquivo como multipart/form-data (campo "arquivo") e devolve a resposta.</summary>
    public Task<T> PostArquivoAsync<T>(
        string rota,
        Stream conteudo,
        string nomeArquivo,
        string? tipoConteudo,
        CancellationToken ct = default)
    {
        return EnviarAsync<T>(
            () =>
            {
                var formulario = new MultipartFormDataContent();
                var arquivo = new StreamContent(conteudo);
                arquivo.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(tipoConteudo) ? "application/octet-stream" : tipoConteudo);

                formulario.Add(arquivo, "arquivo", nomeArquivo);

                return new HttpRequestMessage(HttpMethod.Post, AppConfig.Url(rota)) { Content = formulario };
            },
            tentativas: 1,
            ct);
    }

    public Task<T> DeleteAsync<T>(string rota, CancellationToken ct = default)
        => EnviarAsync<T>(() => new HttpRequestMessage(HttpMethod.Delete, AppConfig.Url(rota)), tentativas: 1, ct);

    /// <summary>
    /// Baixa uma imagem da API. Passa pelo mesmo cliente das demais chamadas
    /// (token do usuário e cabeçalho do ngrok), diferente do carregador nativo de imagens.
    /// </summary>
    public async Task<byte[]> BaixarImagemAsync(string caminhoRelativo, CancellationToken ct = default)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, AppConfig.Url(caminhoRelativo));
        using var resposta = await ExecutarAsync(requisicao, ct);

        if (!resposta.IsSuccessStatusCode)
            throw await ApiException.DeRespostaAsync(resposta, ct);

        return await resposta.Content.ReadAsByteArrayAsync(ct);
    }

    private async Task<T> EnviarAsync<T>(Func<HttpRequestMessage> criarRequisicao, int tentativas, CancellationToken ct)
    {
        for (var tentativa = 1; ; tentativa++)
        {
            using var requisicao = criarRequisicao();

            try
            {
                using var resposta = await ExecutarAsync(requisicao, ct);

                if (!resposta.IsSuccessStatusCode)
                    throw await ApiException.DeRespostaAsync(resposta, ct);

                if (requisicao.Method != HttpMethod.Get)
                    Interlocked.Increment(ref _versaoDados);

                var json = await resposta.Content.ReadAsStringAsync(ct);
                return string.IsNullOrWhiteSpace(json)
                    ? default!
                    : JsonSerializer.Deserialize<T>(json, Json)!;
            }
            catch (ApiException ex) when (ex.SemConexao && tentativa < tentativas && !ct.IsCancellationRequested)
            {
                // Rede instável: espera um instante e tenta de novo (mesmo corpo, mesmo id de operação).
                await Task.Delay(EsperaEntreTentativas * tentativa, ct);
            }
        }
    }

    private async Task<HttpResponseMessage> ExecutarAsync(HttpRequestMessage requisicao, CancellationToken ct)
    {
        var token = await _sessao.ObterTokenValidoAsync(ct);
        if (!string.IsNullOrWhiteSpace(token))
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Com o túnel ngrok (plano gratuito) o navegador vê uma página de aviso;
        // este cabeçalho garante que o app receba sempre o JSON da API.
        requisicao.Headers.TryAddWithoutValidation("ngrok-skip-browser-warning", "true");

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
