namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Guarda as imagens (fotos de perfil e de produtos) em disco e devolve o caminho
/// relativo usado pela API (ex.: "imagens/abc123.jpg"), servido como arquivo estático.
/// </summary>
public interface IArmazenamentoImagens
{
    /// <summary>Salva a imagem e devolve o caminho relativo (opcionalmente numa subpasta).</summary>
    Task<string> SalvarAsync(
        Stream conteudo,
        string extensao,
        string? subpasta = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma imagem pelo caminho relativo, se ela existir e tiver sido gerada pela
    /// própria API (as imagens-base que acompanham o projeto nunca são apagadas).
    /// </summary>
    void Remover(string? caminhoRelativo);
}
