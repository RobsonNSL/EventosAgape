using AgapeApp.Application.Relatorios.DTOs;
using MediatR;

namespace AgapeApp.Application.Relatorios.Queries.ListarSaldosDoEvento;

/// <summary>
/// Lista os cartões com saldo ainda disponível no evento (saldo em circulação).
/// Somente administradores.
/// </summary>
public record ListarSaldosDoEventoQuery(Guid EventoId) : IRequest<IReadOnlyList<SaldoEmCirculacaoDTO>>;
