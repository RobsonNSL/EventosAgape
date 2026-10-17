namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Guarda as fotos de perfil em disco e devolve o caminho relativo usado pela API
/// (ex.: "imagens/abc123.jpg"), servido como arquivo estático.
/// </summary>
public interface IArmazenamentoImagens
{
    /// <summary>Salva a imagem e devolve o caminho relativo.</summary>
    Task<string> SalvarAsync(
        Stream conteudo,
        string extensao,
        CancellationToken cancellationToken = default);

    /// <summary>Remove uma imagem pelo caminho relativo, se ela existir.</summary>
    void Remover(string? caminhoRelativo);
}
