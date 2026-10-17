namespace AgapeApp.Application.Common.DTOs;

/// <summary>
/// Arquivo binário pronto para download (ex.: exportação em Excel).
/// </summary>
public record ArquivoExportadoDTO(
    string NomeArquivo,
    string TipoConteudo,
    byte[] Conteudo);
