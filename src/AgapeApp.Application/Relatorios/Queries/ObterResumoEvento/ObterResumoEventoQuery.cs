using AgapeApp.Application.Relatorios.DTOs;
using MediatR;

namespace AgapeApp.Application.Relatorios.Queries.ObterResumoEvento;

/// <summary>
/// Monta o resumo do evento (totais, saldo em circulação e consolidados por setor,
/// operador e produto). Somente administradores.
/// </summary>
public record ObterResumoEventoQuery(Guid EventoId) : IRequest<ResumoEventoDTO>;
