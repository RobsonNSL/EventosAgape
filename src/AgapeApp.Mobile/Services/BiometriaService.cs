using Plugin.Maui.Biometric;

namespace AgapeApp.Mobile.Services;

/// <summary>
/// Biometria do aparelho (impressão digital / Face ID / Windows Hello).
/// Serve para reabrir a sessão salva sem digitar a senha; o token de renovação
/// continua guardado no SecureStorage.
/// </summary>
public class BiometriaService
{
    private const string ChaveAtiva = "agape.biometria_ativa";

    private readonly IBiometric _biometria;

    public BiometriaService(IBiometric biometria) => _biometria = biometria;

    /// <summary>O usuário aceitou usar a biometria neste aparelho.</summary>
    public bool Ativada
    {
        get => Preferences.Default.Get(ChaveAtiva, false);
        set => Preferences.Default.Set(ChaveAtiva, value);
    }

    /// <summary>Há biometria cadastrada e pronta para uso.</summary>
    public async Task<bool> DisponivelAsync()
    {
        try
        {
            if (!_biometria.IsPlatformSupported)
                return false;

            var status = await _biometria.GetAuthenticationStatusAsync();
            return status == BiometricHwStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Pede a biometria ao usuário; devolve true quando reconhecido.</summary>
    public async Task<bool> AutenticarAsync(string motivo, CancellationToken ct = default)
    {
        try
        {
            var resposta = await _biometria.AuthenticateAsync(new AuthenticationRequest
            {
                Title = "AgapeApp",
                Subtitle = motivo,
                Description = motivo,
                NegativeText = "Usar senha",
                AllowPasswordAuth = false
            }, ct);

            return resposta.Status == BiometricResponseStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}
