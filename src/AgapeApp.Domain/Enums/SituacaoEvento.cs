namespace AgapeApp.Domain.Enums;

/// <summary>
/// Ciclo de vida do evento: Rascunho → Aberto → Encerrado (aberto/encerrado manualmente pelo admin).
/// </summary>
public enum SituacaoEvento
{
    /// <summary>Em preparação: o admin monta setores, produtos e operadores.</summary>
    Rascunho = 1,

    /// <summary>Aberto: recargas e vendas liberadas para os operadores designados.</summary>
    Aberto = 2,

    /// <summary>Encerrado: nenhuma operação nova; apenas consultas e relatórios.</summary>
    Encerrado = 3
}
