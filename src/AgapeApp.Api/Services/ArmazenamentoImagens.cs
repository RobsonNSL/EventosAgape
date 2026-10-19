using AgapeApp.Application.Interfaces;

namespace AgapeApp.Api.Services;

/// <summary>
/// Guarda as imagens em <c>wwwroot/imagens</c> (fotos de perfil na raiz, produtos em
/// <c>imagens/produtos</c>), servidas como arquivos estáticos. O banco guarda só o caminho relativo.
/// </summary>
public class ArmazenamentoImagens : IArmazenamentoImagens
{
    /// <summary>Nome da pasta pública das imagens (relativa ao wwwroot).</summary>
    public const string Pasta = "imagens";

    private readonly string _pastaFisica;

    public ArmazenamentoImagens(IWebHostEnvironment ambiente)
    {
        _pastaFisica = Path.GetFullPath(Path.Combine(ambiente.ContentRootPath, "wwwroot", Pasta));
        Directory.CreateDirectory(_pastaFisica);
    }

    public async Task<string> SalvarAsync(
        Stream conteudo,
        string extensao,
        string? subpasta = null,
        CancellationToken cancellationToken = default)
    {
        // Só letras/números na subpasta: nada de "..", barras ou caminhos absolutos.
        var pasta = string.IsNullOrWhiteSpace(subpasta) || !subpasta.All(char.IsLetterOrDigit)
            ? null
            : subpasta.ToLowerInvariant();

        var destinoPasta = pasta is null ? _pastaFisica : Path.Combine(_pastaFisica, pasta);
        Directory.CreateDirectory(destinoPasta);

        // Nome novo a cada troca: evita cache de imagem antiga no aparelho.
        var nomeArquivo = $"{Guid.NewGuid():N}.{extensao}";

        await using (var arquivo = File.Create(Path.Combine(destinoPasta, nomeArquivo)))
            await conteudo.CopyToAsync(arquivo, cancellationToken);

        return pasta is null ? $"{Pasta}/{nomeArquivo}" : $"{Pasta}/{pasta}/{nomeArquivo}";
    }

    public void Remover(string? caminhoRelativo)
    {
        if (string.IsNullOrWhiteSpace(caminhoRelativo))
            return;

        try
        {
            // Só apaga arquivos com nome gerado pela API (GUID): as imagens-base dos produtos
            // (ex.: "coca-cola-lata.jpg") acompanham o projeto e nunca saem do disco.
            if (!Guid.TryParseExact(Path.GetFileNameWithoutExtension(caminhoRelativo), "N", out _))
                return;

            var semPrefixo = caminhoRelativo.Replace('\\', '/').TrimStart('/');
            if (semPrefixo.StartsWith($"{Pasta}/", StringComparison.OrdinalIgnoreCase))
                semPrefixo = semPrefixo[(Pasta.Length + 1)..];

            // O caminho final precisa continuar dentro da pasta de imagens.
            var arquivo = Path.GetFullPath(Path.Combine(_pastaFisica, semPrefixo));
            if (!arquivo.StartsWith(_pastaFisica + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return;

            if (File.Exists(arquivo))
                File.Delete(arquivo);
        }
        catch (IOException)
        {
            // A imagem antiga pode ficar órfã no disco; isso não impede a troca.
        }
        catch (UnauthorizedAccessException)
        {
            // Idem: a troca já foi concluída no banco.
        }
    }
}
