using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Recargas.DTOs;
using MediatR;

namespace AgapeApp.Application.Recargas.Queries.ListarRecargas;

/// <summary>
/// Lista as recargas de um evento (mais recentes primeiro), de forma paginada.
/// Somente administradores.
/// </summary>
public record ListarRecargasQuery(
    Guid EventoId,
    int Pagina = 1,
    int TamanhoPagina = 50) : IRequest<ResultadoPaginadoDTO<RecargaDTO>>;
