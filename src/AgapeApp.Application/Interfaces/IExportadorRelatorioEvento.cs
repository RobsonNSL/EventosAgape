using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Relatorios.DTOs;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Gera a planilha Excel do relatório de um evento (implementado na Infraestrutura com ClosedXML).
/// </summary>
public interface IExportadorRelatorioEvento
{
    ArquivoExportadoDTO GerarExcelDoEvento(
        ResumoEventoDTO resumo,
        IReadOnlyList<SaldoEmCirculacaoDTO> saldos);
}
