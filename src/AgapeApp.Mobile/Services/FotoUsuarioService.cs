using AgapeApp.Mobile.Models;

namespace AgapeApp.Mobile.Services;

/// <summary>
/// Foto de perfil do usuário. A imagem é baixada pela API (com o token e o cabeçalho
/// do ngrok, que o carregador nativo de imagens não envia) e fica em memória: abrir a
/// lista de eventos ou o perfil não baixa a mesma foto de novo a cada vez.
/// Sem foto — ou sem conexão — a tela segue com as iniciais.
/// </summary>
public class FotoUsuarioService
{
    /// <summary>
    /// Opções do seletor/câmera: a foto já sai reduzida e comprimida (as câmeras atuais
    /// geram arquivos de vários MB, acima do limite da API e lentos no 4G).
    /// </summary>
    public static MediaPickerOptions OpcoesCaptura(string titulo) => new()
    {
        Title = titulo,
        SelectionLimit = 1,
        MaximumWidth = 720,
        MaximumHeight = 720,
        CompressionQuality = 85,
        RotateImage = true
    };

    private readonly ApiClient _api;
    private readonly Dictionary<string, byte[]> _cache = [];

    public FotoUsuarioService(ApiClient api) => _api = api;

    public async Task<ImageSource?> CarregarAsync(string? caminhoFoto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(caminhoFoto))
            return null;

        try
        {
            if (!_cache.TryGetValue(caminhoFoto, out var bytes))
            {
                bytes = await _api.BaixarImagemAsync(caminhoFoto, ct);
                _cache[caminhoFoto] = bytes;
            }

            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        catch (Exception)
        {
            // Foto indisponível (offline ou arquivo ausente): as iniciais continuam valendo.
            return null;
        }
    }

    /// <summary>
    /// Preenche as imagens de uma lista já exibida (3 downloads por vez, com cache em memória):
    /// a lista aparece na hora com o ícone e as imagens chegam em seguida, sem travar a tela.
    /// </summary>
    public async Task CarregarEmLoteAsync(IEnumerable<IComImagem> itens)
    {
        try
        {
            using var limite = new SemaphoreSlim(3);

            await Task.WhenAll(itens
                .Where(item => item.Imagem is null && !string.IsNullOrWhiteSpace(item.CaminhoImagem))
                .Select(async item =>
                {
                    await limite.WaitAsync();
                    try
                    {
                        var imagem = await CarregarAsync(item.CaminhoImagem);
                        if (imagem is not null)
                            MainThread.BeginInvokeOnMainThread(() => item.Imagem = imagem);
                    }
                    finally
                    {
                        limite.Release();
                    }
                }));
        }
        catch (Exception)
        {
            // Imagem é enfeite: sem ela continua o ícone.
        }
    }

    /// <summary>Esquece as fotos em memória (depois de trocar ou remover).</summary>
    public void Limpar() => _cache.Clear();
}
