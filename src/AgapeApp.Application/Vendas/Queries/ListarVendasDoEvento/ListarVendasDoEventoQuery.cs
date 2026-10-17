using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Vendas.DTOs;
using MediatR;

namespace AgapeApp.Application.Vendas.Queries.ListarVendasDoEvento;

/// <summary>
/// Lista as vendas de um evento (mais recentes primeiro), de forma paginada.
/// Somente administradores.
/// </summary>
public record ListarVendasDoEventoQuery(
    Guid EventoId,
    int Pagina = 1,
    int TamanhoPagina = 50) : IRequest<ResultadoPaginadoDTO<VendaDTO>>;
