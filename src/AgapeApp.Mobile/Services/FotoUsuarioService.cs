namespace AgapeApp.Mobile.Services;

/// <summary>
/// Carrega a foto de perfil do usuário. A imagem é baixada pela API (com o token e o
/// cabeçalho do ngrok, que o carregador nativo de imagens não envia) e devolvida como
/// <see cref="ImageSource"/>; sem foto — ou sem conexão — a tela segue com as iniciais.
/// </summary>
public class FotoUsuarioService
{
    private readonly ApiClient _api;

    public FotoUsuarioService(ApiClient api) => _api = api;

    public async Task<ImageSource?> CarregarAsync(string? caminhoFoto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(caminhoFoto))
            return null;

        try
        {
            var bytes = await _api.BaixarImagemAsync(caminhoFoto, ct);
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        catch (Exception)
        {
            // Foto indisponível (offline ou arquivo ausente): as iniciais continuam valendo.
            return null;
        }
    }
}
