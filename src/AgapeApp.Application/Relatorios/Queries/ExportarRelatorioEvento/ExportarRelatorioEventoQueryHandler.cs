using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Relatorios.Queries.ListarSaldosDoEvento;
using AgapeApp.Application.Relatorios.Queries.ObterResumoEvento;
using MediatR;

namespace AgapeApp.Application.Relatorios.Queries.ExportarRelatorioEvento;

public class ExportarRelatorioEventoQueryHandler
    : IRequestHandler<ExportarRelatorioEventoQuery, ArquivoExportadoDTO>
{
    private readonly ISender _mediator;
    private readonly IExportadorRelatorioEvento _exportadorRelatorioEvento;

    public ExportarRelatorioEventoQueryHandler(
        ISender mediator,
        IExportadorRelatorioEvento exportadorRelatorioEvento)
    {
        _mediator = mediator;
        _exportadorRelatorioEvento = exportadorRelatorioEvento;
    }

    public async Task<ArquivoExportadoDTO> Handle(
        ExportarRelatorioEventoQuery consulta,
        CancellationToken cancellationToken)
    {
        // Reaproveita as consultas do resumo e dos saldos (mesmas regras e validações da tela).
        var resumo = await _mediator.Send(new ObterResumoEventoQuery(consulta.EventoId), cancellationToken);
        var saldos = await _mediator.Send(new ListarSaldosDoEventoQuery(consulta.EventoId), cancellationToken);

        return _exportadorRelatorioEvento.GerarExcelDoEvento(resumo, saldos);
    }
}
