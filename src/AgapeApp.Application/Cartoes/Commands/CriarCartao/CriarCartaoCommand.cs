using AgapeApp.Application.Cartoes.DTOs;
using MediatR;

namespace AgapeApp.Application.Cartoes.Commands.CriarCartao;

/// <summary>
/// Cadastra um novo cartão (um a um). O código é normalizado (maiúsculas) e deve ser único.
/// Somente administradores.
/// </summary>
public record CriarCartaoCommand(string Codigo) : IRequest<CartaoDTO>;
