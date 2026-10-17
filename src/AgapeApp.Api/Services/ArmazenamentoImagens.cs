using AgapeApp.Application.Interfaces;

namespace AgapeApp.Api.Services;

/// <summary>
/// Guarda as fotos de perfil em <c>wwwroot/imagens</c>, servidas como arquivos
/// estáticos em <c>/imagens/{arquivo}</c>. O banco guarda só o caminho relativo.
/// </summary>
public class ArmazenamentoImagens : IArmazenamentoImagens
{
    /// <summary>Nome da pasta pública das imagens (relativa ao wwwroot).</summary>
    public const string Pasta = "imagens";

    private readonly string _pastaFisica;

    public ArmazenamentoImagens(IWebHostEnvironment ambiente)
    {
        _pastaFisica = Path.Combine(ambiente.ContentRootPath, "wwwroot", Pasta);
        Directory.CreateDirectory(_pastaFisica);
    }

    public async Task<string> SalvarAsync(
        Stream conteudo,
        string extensao,
        CancellationToken cancellationToken = default)
    {
        // Nome novo a cada troca: evita cache de imagem antiga no aparelho.
        var nomeArquivo = $"{Guid.NewGuid():N}.{extensao}";
        var destino = Path.Combine(_pastaFisica, nomeArquivo);

        await using (var arquivo = File.Create(destino))
            await conteudo.CopyToAsync(arquivo, cancellationToken);

        return $"{Pasta}/{nomeArquivo}";
    }

    public void Remover(string? caminhoRelativo)
    {
        if (string.IsNullOrWhiteSpace(caminhoRelativo))
            return;

        try
        {
            // Só o nome do arquivo é considerado (protege contra caminhos arbitrários).
            var arquivo = Path.Combine(_pastaFisica, Path.GetFileName(caminhoRelativo));

            if (File.Exists(arquivo))
                File.Delete(arquivo);
        }
        catch (IOException)
        {
            // A imagem antiga pode ficar órfã no disco; isso não impede a troca da foto.
        }
        catch (UnauthorizedAccessException)
        {
            // Idem: a troca da foto já foi concluída no banco.
        }
    }
}
