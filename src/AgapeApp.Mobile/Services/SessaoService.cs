using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using AgapeApp.Mobile.Config;
using AgapeApp.Mobile.Models;

namespace AgapeApp.Mobile.Services;

/// <summary>
/// Sessão do usuário: login, renovação automática do token de acesso e logout.
/// Os tokens ficam no SecureStorage do aparelho (Keychain/Keystore).
/// </summary>
public class SessaoService
{
    private const string ChaveAcesso = "agape.token.acesso";
    private const string ChaveRenovacao = "agape.token.renovacao";
    private const string ChaveExpiraEm = "agape.token.expira_em";
    private const string ChaveUsuario = "agape.usuario";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan MargemRenovacao = TimeSpan.FromMinutes(1);

    private readonly HttpClient _http;
    private readonly SemaphoreSlim _trava = new(1, 1);

    private string? _tokenAcesso;
    private string? _tokenRenovacao;
    private DateTimeOffset _expiraEm = DateTimeOffset.MinValue;

    public SessaoService(HttpClient http) => _http = http;

    public UsuarioAutenticadoDTO? Usuario { get; private set; }

    public bool Autenticado => !string.IsNullOrWhiteSpace(_tokenAcesso) && Usuario is not null;

    public bool EhAdministrador => Usuario?.Perfil == "Administrador";

    /// <summary>Há token de renovação salvo (permite reabrir a sessão com biometria).</summary>
    public bool TemRenovacaoSalva => !string.IsNullOrWhiteSpace(_tokenRenovacao);

    /// <summary>Restaura a sessão salva no aparelho (chamado na abertura do app).</summary>
    public async Task<bool> CarregarAsync()
    {
        try
        {
            _tokenAcesso = await SecureStorage.Default.GetAsync(ChaveAcesso);
            _tokenRenovacao = await SecureStorage.Default.GetAsync(ChaveRenovacao);

            var expira = await SecureStorage.Default.GetAsync(ChaveExpiraEm);
            if (DateTimeOffset.TryParse(expira, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var valor))
                _expiraEm = valor;

            var usuario = await SecureStorage.Default.GetAsync(ChaveUsuario);
            if (!string.IsNullOrWhiteSpace(usuario))
                Usuario = JsonSerializer.Deserialize<UsuarioAutenticadoDTO>(usuario, Json);

            return Autenticado;
        }
        catch
        {
            await LimparAsync();
            return false;
        }
    }

    /// <summary>Atualiza a foto de perfil do usuário na sessão (depois de trocar ou remover).</summary>
    public async Task AtualizarFotoAsync(string? caminhoFoto)
    {
        if (Usuario is null)
            return;

        Usuario = Usuario with { CaminhoFoto = caminhoFoto };

        try
        {
            await SecureStorage.Default.SetAsync(ChaveUsuario, JsonSerializer.Serialize(Usuario, Json));
        }
        catch
        {
            // Sem armazenamento seguro disponível: a foto vale só enquanto o app estiver aberto.
        }
    }

    /// <summary>Autentica com e-mail e senha e guarda a sessão.</summary>
    public async Task EntrarAsync(string email, string senha, CancellationToken ct = default)
    {
        var resposta = await EnviarAsync<LoginResponse>(
            "api/autenticacao/login",
            new LoginRequest(email, senha, AppConfig.Dispositivo),
            autenticado: false,
            ct);

        await SalvarAsync(resposta);
    }

    /// <summary>
    /// Renova a sessão usando somente o token de renovação salvo no aparelho.
    /// Usado na entrada por biometria: se o token foi revogado ou expirou, devolve false.
    /// </summary>
    public async Task<bool> RenovarSessaoSalvaAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_tokenRenovacao))
            return false;

        await _trava.WaitAsync(ct);
        try
        {
            var resposta = await EnviarAsync<LoginResponse>(
                "api/autenticacao/renovar",
                new RenovarTokenRequest(_tokenRenovacao, AppConfig.Dispositivo),
                autenticado: false,
                ct);

            await SalvarAsync(resposta);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _trava.Release();
        }
    }

    /// <summary>Encerra a sessão no servidor (revoga a renovação) e apaga os tokens locais.</summary>
    public async Task SairAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_tokenRenovacao))
            {
                await EnviarAsync<object?>(
                    "api/autenticacao/sair",
                    new SairRequest(_tokenRenovacao),
                    autenticado: true,
                    CancellationToken.None);
            }
        }
        catch (ApiException)
        {
            // Saída sempre funciona localmente, mesmo com servidor fora do ar.
        }
        finally
        {
            await LimparAsync();
        }
    }

    /// <summary>
    /// Devolve um token de acesso válido, renovando automaticamente quando
    /// estiver perto de expirar. Sem sessão válida, devolve null.
    /// </summary>
    public async Task<string?> ObterTokenValidoAsync(CancellationToken ct = default)
    {
        if (!Autenticado)
            return null;

        if (_expiraEm - DateTimeOffset.UtcNow > MargemRenovacao)
            return _tokenAcesso;

        await _trava.WaitAsync(ct);
        try
        {
            if (_expiraEm - DateTimeOffset.UtcNow > MargemRenovacao)
                return _tokenAcesso;

            if (string.IsNullOrWhiteSpace(_tokenRenovacao))
            {
                await LimparAsync();
                return null;
            }

            var resposta = await EnviarAsync<LoginResponse>(
                "api/autenticacao/renovar",
                new RenovarTokenRequest(_tokenRenovacao, AppConfig.Dispositivo),
                autenticado: false,
                ct);

            await SalvarAsync(resposta);
            return _tokenAcesso;
        }
        catch
        {
            // Renovação falhou (token revogado/expirado): exige novo login.
            await LimparAsync();
            return null;
        }
        finally
        {
            _trava.Release();
        }
    }

    /// <summary>
    /// Abertura do app: ninguém entra direto. Com biometria ativada, fica guardado só o
    /// token de renovação (a digital troca por um acesso novo); sem biometria, a sessão
    /// salva é apagada e o login pede e-mail e senha.
    /// </summary>
    public Task PrepararAberturaAsync(bool manterParaBiometria)
        => _abertura = PrepararAberturaInternoAsync(manterParaBiometria);

    /// <summary>A tela de login espera a abertura terminar antes de decidir se oferece a biometria.</summary>
    public Task AguardarAberturaAsync() => _abertura ?? Task.CompletedTask;

    private Task? _abertura;

    private async Task PrepararAberturaInternoAsync(bool manterParaBiometria)
    {
        if (!manterParaBiometria)
        {
            await LimparAsync();
            return;
        }

        await CarregarAsync();

        _tokenAcesso = null;
        _expiraEm = DateTimeOffset.MinValue;
        RemoverSeguro(ChaveAcesso);
        RemoverSeguro(ChaveExpiraEm);
    }

    /// <summary>Apaga a sessão local.</summary>
    public Task LimparAsync()
    {
        _tokenAcesso = null;
        _tokenRenovacao = null;
        _expiraEm = DateTimeOffset.MinValue;
        Usuario = null;

        RemoverSeguro(ChaveAcesso);
        RemoverSeguro(ChaveRenovacao);
        RemoverSeguro(ChaveExpiraEm);
        RemoverSeguro(ChaveUsuario);

        return Task.CompletedTask;
    }

    private async Task<T> EnviarAsync<T>(string rota, object corpo, bool autenticado, CancellationToken ct)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, AppConfig.Url(rota))
        {
            Content = new StringContent(JsonSerializer.Serialize(corpo, Json), Encoding.UTF8, "application/json")
        };

        if (autenticado && !string.IsNullOrWhiteSpace(_tokenAcesso))
            requisicao.Headers.Authorization = new("Bearer", _tokenAcesso);

        HttpResponseMessage resposta;
        try
        {
            resposta = await _http.SendAsync(requisicao, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested))
        {
            throw new ApiException(
                HttpStatusCode.ServiceUnavailable,
                "Sem conexão com o servidor. Verifique a rede e tente novamente.");
        }

        using (resposta)
        {
            if (!resposta.IsSuccessStatusCode)
                throw await ApiException.DeRespostaAsync(resposta, ct);

            var json = await resposta.Content.ReadAsStringAsync(ct);
            return string.IsNullOrWhiteSpace(json)
                ? default!
                : JsonSerializer.Deserialize<T>(json, Json)!;
        }
    }

    private async Task SalvarAsync(LoginResponse resposta)
    {
        _tokenAcesso = resposta.TokenAcesso;
        _tokenRenovacao = resposta.TokenRenovacao;
        _expiraEm = resposta.ExpiraEm;
        Usuario = resposta.Usuario;

        try
        {
            await SecureStorage.Default.SetAsync(ChaveAcesso, _tokenAcesso);
            await SecureStorage.Default.SetAsync(ChaveRenovacao, _tokenRenovacao);
            await SecureStorage.Default.SetAsync(ChaveExpiraEm, _expiraEm.ToString("O", CultureInfo.InvariantCulture));
            await SecureStorage.Default.SetAsync(ChaveUsuario, JsonSerializer.Serialize(Usuario, Json));
        }
        catch
        {
            // Sem armazenamento seguro disponível: a sessão vale só enquanto o app estiver aberto.
        }
    }

    private static void RemoverSeguro(string chave)
    {
        try
        {
            SecureStorage.Default.Remove(chave);
        }
        catch
        {
            // Ignora falha ao limpar o armazenamento seguro.
        }
    }
}
