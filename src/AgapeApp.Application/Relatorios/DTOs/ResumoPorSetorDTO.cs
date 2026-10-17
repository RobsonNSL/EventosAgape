namespace AgapeApp.Application.Relatorios.DTOs;

/// <summary>
/// Consolidado de um setor do evento (recargas e vendas).
/// </summary>
public record ResumoPorSetorDTO(
    Guid SetorId,
    string SetorNome,
    int QuantidadeRecargas,
    decimal TotalRecargas,
    int QuantidadeVendas,
    decimal TotalVendas);
