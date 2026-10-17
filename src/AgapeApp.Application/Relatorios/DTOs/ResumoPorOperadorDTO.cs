namespace AgapeApp.Application.Relatorios.DTOs;

/// <summary>
/// Consolidado de um operador do evento (recargas e vendas).
/// </summary>
public record ResumoPorOperadorDTO(
    Guid UsuarioId,
    string UsuarioNome,
    int QuantidadeRecargas,
    decimal TotalRecargas,
    int QuantidadeVendas,
    decimal TotalVendas);
