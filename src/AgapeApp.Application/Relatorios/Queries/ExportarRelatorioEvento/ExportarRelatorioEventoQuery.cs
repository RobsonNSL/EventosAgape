using AgapeApp.Application.Common.DTOs;
using MediatR;

namespace AgapeApp.Application.Relatorios.Queries.ExportarRelatorioEvento;

/// <summary>
/// Exporta o relatório do evento em Excel (resumo, consolidados e saldos em circulação).
/// Somente administradores.
/// </summary>
public record ExportarRelatorioEventoQuery(Guid EventoId) : IRequest<ArquivoExportadoDTO>;
