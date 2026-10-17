using System.Globalization;
using System.Text;
using AgapeApp.Application.Common.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Relatorios.DTOs;
using ClosedXML.Excel;

namespace AgapeApp.Infrastructure.Relatorios;

/// <summary>
/// Gera a planilha do relatório do evento com uma aba por visão:
/// Resumo, Por setor, Por operador, Por produto e Saldos em circulação.
/// </summary>
public class ExportadorRelatorioEventoExcel : IExportadorRelatorioEvento
{
    private const string FormatoMoeda = "R$ #,##0.00";

    public ArquivoExportadoDTO GerarExcelDoEvento(
        ResumoEventoDTO resumo,
        IReadOnlyList<SaldoEmCirculacaoDTO> saldos)
    {
        using var workbook = new XLWorkbook();

        MontarResumo(workbook, resumo);
        MontarPorSetor(workbook, resumo.PorSetor);
        MontarPorOperador(workbook, resumo.PorOperador);
        MontarPorProduto(workbook, resumo.PorProduto);
        MontarSaldos(workbook, saldos);

        using var memoria = new MemoryStream();
        workbook.SaveAs(memoria);

        return new ArquivoExportadoDTO(
            $"relatorio-{GerarSlug(resumo.EventoNome)}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            memoria.ToArray());
    }

    private static void MontarResumo(XLWorkbook workbook, ResumoEventoDTO resumo)
    {
        var planilha = workbook.Worksheets.Add("Resumo");

        EscreverLinha(planilha, 1, "Evento", resumo.EventoNome);
        EscreverLinha(planilha, 2, "Situação", resumo.Situacao);

        EscreverLinha(planilha, 4, "Recargas (quantidade)", resumo.QuantidadeRecargas);
        EscreverLinha(planilha, 5, "Recargas (total)", resumo.TotalRecargas);
        EscreverLinha(planilha, 6, "Vendas (quantidade)", resumo.QuantidadeVendas);
        EscreverLinha(planilha, 7, "Vendas (total)", resumo.TotalVendas);
        EscreverLinha(planilha, 8, "Saldo em circulação", resumo.SaldoEmCirculacao);

        EscreverLinhaCabecalhoLinha(planilha, 1);
        EscreverLinhaCabecalhoLinha(planilha, 2);
        EscreverLinhaCabecalhoLinha(planilha, 4);
        EscreverLinhaCabecalhoLinha(planilha, 5);
        EscreverLinhaCabecalhoLinha(planilha, 6);
        EscreverLinhaCabecalhoLinha(planilha, 7);
        EscreverLinhaCabecalhoLinha(planilha, 8);

        AplicarMoeda(planilha.Cell(5, 2));
        AplicarMoeda(planilha.Cell(7, 2));
        AplicarMoeda(planilha.Cell(8, 2));

        planilha.Column(1).AdjustToContents();
        planilha.Column(2).AdjustToContents();
    }

    private static void MontarPorSetor(XLWorkbook workbook, IReadOnlyList<ResumoPorSetorDTO> porSetor)
    {
        var planilha = workbook.Worksheets.Add("Por setor");

        EscreverCabecalho(
            planilha,
            "Setor",
            "Recargas (qtd)",
            "Recargas (total)",
            "Vendas (qtd)",
            "Vendas (total)");

        var linha = 2;
        foreach (var item in porSetor)
        {
            planilha.Cell(linha, 1).Value = item.SetorNome;
            planilha.Cell(linha, 2).Value = item.QuantidadeRecargas;
            planilha.Cell(linha, 3).Value = item.TotalRecargas;
            planilha.Cell(linha, 4).Value = item.QuantidadeVendas;
            planilha.Cell(linha, 5).Value = item.TotalVendas;
            linha++;
        }

        AplicarMoedaColuna(planilha, 2, linha - 1, 3);
        AplicarMoedaColuna(planilha, 2, linha - 1, 5);
        planilha.Columns().AdjustToContents();
    }

    private static void MontarPorOperador(XLWorkbook workbook, IReadOnlyList<ResumoPorOperadorDTO> porOperador)
    {
        var planilha = workbook.Worksheets.Add("Por operador");

        EscreverCabecalho(
            planilha,
            "Operador",
            "Recargas (qtd)",
            "Recargas (total)",
            "Vendas (qtd)",
            "Vendas (total)");

        var linha = 2;
        foreach (var item in porOperador)
        {
            planilha.Cell(linha, 1).Value = item.UsuarioNome;
            planilha.Cell(linha, 2).Value = item.QuantidadeRecargas;
            planilha.Cell(linha, 3).Value = item.TotalRecargas;
            planilha.Cell(linha, 4).Value = item.QuantidadeVendas;
            planilha.Cell(linha, 5).Value = item.TotalVendas;
            linha++;
        }

        AplicarMoedaColuna(planilha, 2, linha - 1, 3);
        AplicarMoedaColuna(planilha, 2, linha - 1, 5);
        planilha.Columns().AdjustToContents();
    }

    private static void MontarPorProduto(XLWorkbook workbook, IReadOnlyList<ResumoPorProdutoDTO> porProduto)
    {
        var planilha = workbook.Worksheets.Add("Por produto");

        EscreverCabecalho(planilha, "Produto", "Quantidade vendida", "Total vendido");

        var linha = 2;
        foreach (var item in porProduto)
        {
            planilha.Cell(linha, 1).Value = item.ProdutoNome;
            planilha.Cell(linha, 2).Value = item.QuantidadeVendida;
            planilha.Cell(linha, 3).Value = item.TotalVendido;
            linha++;
        }

        AplicarMoedaColuna(planilha, 2, linha - 1, 3);
        planilha.Columns().AdjustToContents();
    }

    private static void MontarSaldos(XLWorkbook workbook, IReadOnlyList<SaldoEmCirculacaoDTO> saldos)
    {
        var planilha = workbook.Worksheets.Add("Saldos em circulação");

        EscreverCabecalho(planilha, "Cartão", "Saldo");

        var linha = 2;
        foreach (var saldo in saldos)
        {
            planilha.Cell(linha, 1).Value = saldo.CartaoCodigo;
            planilha.Cell(linha, 2).Value = saldo.Saldo;
            linha++;
        }

        AplicarMoedaColuna(planilha, 2, linha - 1, 2);
        planilha.Columns().AdjustToContents();
    }

    private static void EscreverCabecalho(IXLWorksheet planilha, params string[] titulos)
    {
        for (var coluna = 0; coluna < titulos.Length; coluna++)
        {
            var celula = planilha.Cell(1, coluna + 1);
            celula.Value = titulos[coluna];
            celula.Style.Font.Bold = true;
        }
    }

    private static void EscreverLinha(IXLWorksheet planilha, int linha, string rotulo, string valor)
    {
        planilha.Cell(linha, 1).Value = rotulo;
        planilha.Cell(linha, 2).Value = valor;
    }

    private static void EscreverLinha(IXLWorksheet planilha, int linha, string rotulo, decimal valor)
    {
        planilha.Cell(linha, 1).Value = rotulo;
        planilha.Cell(linha, 2).Value = valor;
    }

    private static void EscreverLinha(IXLWorksheet planilha, int linha, string rotulo, int valor)
    {
        planilha.Cell(linha, 1).Value = rotulo;
        planilha.Cell(linha, 2).Value = valor;
    }

    private static void EscreverLinhaCabecalhoLinha(IXLWorksheet planilha, int linha)
        => planilha.Cell(linha, 1).Style.Font.Bold = true;

    private static void AplicarMoeda(IXLCell celula)
        => celula.Style.NumberFormat.Format = FormatoMoeda;

    private static void AplicarMoedaColuna(IXLWorksheet planilha, int linhaInicial, int linhaFinal, int coluna)
    {
        if (linhaFinal < linhaInicial)
            return;

        planilha.Range(linhaInicial, coluna, linhaFinal, coluna).Style.NumberFormat.Format = FormatoMoeda;
    }

    private static string GerarSlug(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var construtor = new StringBuilder();

        foreach (var caractere in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(caractere);

            if (categoria == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(caractere))
                construtor.Append(char.ToLowerInvariant(caractere));
            else if (construtor.Length > 0 && construtor[^1] != '-')
                construtor.Append('-');
        }

        var slug = construtor.ToString().Trim('-');

        return string.IsNullOrEmpty(slug) ? "evento" : slug;
    }
}
