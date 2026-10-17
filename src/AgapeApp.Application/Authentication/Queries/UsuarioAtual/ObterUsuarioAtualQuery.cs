using AgapeApp.Application.Authentication.DTOs;
using MediatR;

namespace AgapeApp.Application.Authentication.Queries.UsuarioAtual;

/// <summary>
/// Consulta os dados do usuário autenticado.
/// </summary>
public record ObterUsuarioAtualQuery(Guid UsuarioId) : IRequest<UsuarioAutenticadoDTO>;
