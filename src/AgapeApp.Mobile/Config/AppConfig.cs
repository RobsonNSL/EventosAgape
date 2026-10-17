namespace AgapeApp.Mobile.Config;

/// <summary>
/// Endereço da API e identificação do aparelho.
/// Desenvolvimento: o emulador Android enxerga o PC como 10.0.2.2; o simulador
/// iOS usa localhost. Aparelho físico (ou produção): defina a URL real do
/// servidor na tela de login ("Configurar endereço do servidor", em DEBUG).
/// </summary>
public static class AppConfig
{
    private const string ChaveApiBaseUrl = "agape.api_base_url";

    public static string ApiBaseUrl
    {
        get
        {
            var personalizada = Preferences.Default.Get(ChaveApiBaseUrl, string.Empty);
            if (!string.IsNullOrWhiteSpace(personalizada))
                return personalizada;

#if ANDROID
            return "http://10.0.2.2:5180";
#else
            return "http://localhost:5180";
#endif
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
