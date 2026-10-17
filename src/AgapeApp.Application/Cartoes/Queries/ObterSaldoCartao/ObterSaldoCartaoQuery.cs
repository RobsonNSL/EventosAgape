using AgapeApp.Application.Cartoes.DTOs;
using MediatR;

namespace AgapeApp.Application.Cartoes.Queries.ObterSaldoCartao;

/// <summary>
/// Consulta o saldo de um cartão em um evento (usada pelo operador após ler o QR code).
/// Cartão ainda sem recarga no evento devolve saldo zero.
/// </summary>
public record ObterSaldoCartaoQuery(
    string Codigo,
    Guid EventoId) : IRequest<CartaoSaldoDTO>;
