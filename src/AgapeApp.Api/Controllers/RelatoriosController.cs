using AgapeApp.Application.Relatorios.DTOs;
using AgapeApp.Application.Relatorios.Queries.ExportarRelatorioEvento;
using AgapeApp.Application.Relatorios.Queries.ListarSaldosDoEvento;
using AgapeApp.Application.Relatorios.Queries.ObterResumoEvento;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Relatórios do evento: resumo geral, saldos em circulação e exportação em Excel.
/// Acesso exclusivo de administradores.
/// </summary>
[ApiController]
[Route("api/eventos/{eventoId:guid}/relatorios")]
[Authorize(Policy = "SomenteAdministrador")]
public class RelatoriosController : ControllerBase
{
    private readonly ISender _mediator;

    public RelatoriosController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Resumo do evento: totais de recargas e vendas, saldo em circulação
    /// e consolidados por setor, operador e produto.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResumoEventoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumoEventoDTO>> Resumo(Guid eventoId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ObterResumoEventoQuery(eventoId), cancellationToken));

    /// <summary>Lista os cartões com saldo ainda disponível no evento (saldo em circulação).</summary>
    [HttpGet("saldos")]
    [ProducesResponseType(typeof(IReadOnlyList<SaldoEmCirculacaoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SaldoEmCirculacaoDTO>>> Saldos(
        Guid eventoId,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarSaldosDoEventoQuery(eventoId), cancellationToken));

    /// <summary>
    /// Baixa o relatório do evento em Excel (abas: Resumo, Por setor, Por operador,
    /// Por produto e Saldos em circulação).
    /// </summary>
    [HttpGet("excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Excel(Guid eventoId, CancellationToken cancellationToken)
    {
        var arquivo = await _mediator.Send(new ExportarRelatorioEventoQuery(eventoId), cancellationToken);

        return File(arquivo.Conteudo, arquivo.TipoConteudo, arquivo.NomeArquivo);
    }
}
