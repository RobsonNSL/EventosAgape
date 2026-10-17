using AgapeApp.Application.Cartoes.DTOs;
using MediatR;

namespace AgapeApp.Application.Cartoes.Queries.ObterCartao;

/// <summary>
/// Obtém um cartão pelo identificador. Somente administradores.
/// </summary>
public record ObterCartaoQuery(Guid Id) : IRequest<CartaoDTO>;
