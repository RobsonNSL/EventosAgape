using AgapeApp.Application.Setores.DTOs;
using MediatR;

namespace AgapeApp.Application.Setores.Queries.ListarSetores;

/// <summary>
/// Lista os setores de um evento. Somente administradores.
/// </summary>
public record ListarSetoresQuery(Guid EventoId) : IRequest<IReadOnlyList<SetorDTO>>;
