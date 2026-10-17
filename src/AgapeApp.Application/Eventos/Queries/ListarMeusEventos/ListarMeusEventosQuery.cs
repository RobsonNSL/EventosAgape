using AgapeApp.Application.Eventos.DTOs;
using MediatR;

namespace AgapeApp.Application.Eventos.Queries.ListarMeusEventos;

/// <summary>
/// Lista os eventos em que o usuário autenticado está designado como operador.
/// O campo UsuarioId é preenchido pela API com o usuário autenticado.
/// </summary>
public record ListarMeusEventosQuery(Guid UsuarioId) : IRequest<IReadOnlyList<MeuEventoDTO>>;
