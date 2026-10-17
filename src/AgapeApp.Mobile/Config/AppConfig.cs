namespace AgapeApp.Mobile.Config;

/// <summary>
/// Endereço da API e identificação do aparelho.
/// Desenvolvimento no aparelho físico: a API local é exposta por um túnel ngrok
/// (HTTPS) — com o túnel ativo, o celular alcança o PC de qualquer rede.
/// Para apontar para outro servidor (emulador, homologação), use
/// "Configurar endereço do servidor" na tela de login (DEBUG).
/// </summary>
public static class AppConfig
{
    private const string ChaveApiBaseUrl = "agape.api_base_url";

    /// <summary>Túnel ngrok que expõe a API local (atualize se o endereço mudar).</summary>
    private const string ApiBaseUrlPadrao = "https://spathic-nonanachronistically-velia.ngrok-free.dev";

    public static string ApiBaseUrl
    {
        get
        {
            var personalizada = Preferences.Default.Get(ChaveApiBaseUrl, string.Empty);
            if (!string.IsNullOrWhiteSpace(personalizada))
                return personalizada;

            return ApiBaseUrlPadrao;
        }
    }

    public static void DefinirApiBaseUrl(string url)
        => Preferences.Default.Set(ChaveApiBaseUrl, url.Trim().TrimEnd('/'));

    public static string Url(string rota)
        => $"{ApiBaseUrl.TrimEnd('/')}/{rota.TrimStart('/')}";

    /// <summary>Identificação amigável do aparelho, gravada nas operações.</summary>
    public static string Dispositivo
        => $"{DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model} · {DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}";
}
