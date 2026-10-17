using AgapeApp.Application.Auditoria.DTOs;
using AgapeApp.Application.Common.DTOs;
using MediatR;

namespace AgapeApp.Application.Auditoria.Queries.ListarAuditoriaDoEvento;

/// <summary>
/// Lista os registros de auditoria de um evento (mais recentes primeiro), de forma paginada.
/// Somente administradores.
/// </summary>
public record ListarAuditoriaDoEventoQuery(
    Guid EventoId,
    int Pagina = 1,
    int TamanhoPagina = 50) : IRequest<ResultadoPaginadoDTO<LogAuditoriaDTO>>;
