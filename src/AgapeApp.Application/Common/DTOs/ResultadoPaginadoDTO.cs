namespace AgapeApp.Application.Common.DTOs;

/// <summary>
/// Resultado paginado de uma listagem.
/// </summary>
public record ResultadoPaginadoDTO<T>(
    IReadOnlyList<T> Itens,
    int Total,
    int Pagina,
    int TamanhoPagina);
