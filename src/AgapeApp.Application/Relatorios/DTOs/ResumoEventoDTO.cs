namespace AgapeApp.Application.Relatorios.DTOs;

/// <summary>
/// Resumo completo do evento: totais de recargas e vendas, saldo em circulação
/// e os consolidados por setor, operador e produto.
/// </summary>
public record ResumoEventoDTO(
    Guid EventoId,
    string EventoNome,
    string Situacao,
    int QuantidadeRecargas,
    decimal TotalRecargas,
    int QuantidadeVendas,
    decimal TotalVendas,
    decimal SaldoEmCirculacao,
    IReadOnlyList<ResumoPorSetorDTO> PorSetor,
    IReadOnlyList<ResumoPorOperadorDTO> PorOperador,
    IReadOnlyList<ResumoPorProdutoDTO> PorProduto);
