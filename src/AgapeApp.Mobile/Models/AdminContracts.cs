namespace AgapeApp.Mobile.Models;

// Contratos de administração (relatórios, saldos e auditoria).
// Espelham os DTOs do AgapeApp.Application.

// ===== Relatórios =====
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

public record ResumoPorSetorDTO(
    Guid SetorId,
    string SetorNome,
    int QuantidadeRecargas,
    decimal TotalRecargas,
    int QuantidadeVendas,
    decimal TotalVendas);

public record ResumoPorOperadorDTO(
    Guid UsuarioId,
    string UsuarioNome,
    int QuantidadeRecargas,
    decimal TotalRecargas,
    int QuantidadeVendas,
    decimal TotalVendas);

public record ResumoPorProdutoDTO(
    Guid EventoProdutoId,
    string ProdutoNome,
    int QuantidadeVendida,
    decimal TotalVendido);

public record SaldoEmCirculacaoDTO(
    Guid CartaoId,
    string CartaoCodigo,
    decimal Saldo);

// ===== Auditoria =====
public record LogAuditoriaDTO(
    Guid Id,
    string UsuarioNome,
    string Acao,
    string Entidade,
    Guid EntidadeId,
    string? Dados,
    string? Dispositivo,
    DateTimeOffset CriadoEm);

public record ResultadoPaginadoDTO<T>(
    IReadOnlyList<T> Itens,
    int Total,
    int Pagina,
    int TamanhoPagina);

// ===== Evento (visão do administrador) =====
public record EventoDTO(
    Guid Id,
    string Nome,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string Situacao,
    DateTimeOffset CriadoEm);
