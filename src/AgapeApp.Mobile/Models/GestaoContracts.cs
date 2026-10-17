namespace AgapeApp.Mobile.Models;

// Contratos JSON das telas de gestão (administrador).
// Espelham os DTOs do AgapeApp.Application.

// ===== Produtos =====
public record ProdutoDTO(
    Guid Id,
    string Nome,
    decimal? PrecoPadrao,
    bool Ativo,
    DateTimeOffset CriadoEm);

// ===== Cartões =====
public record CartaoDTO(
    Guid Id,
    string Codigo,
    bool Ativo,
    DateTimeOffset CriadoEm);

// ===== Produtos do setor =====
public record SetorProdutoDTO(
    Guid Id,
    Guid SetorId,
    Guid EventoProdutoId);

// ===== Operadores do evento =====
public record EventoOperadorDTO(
    Guid Id,
    Guid EventoId,
    Guid UsuarioId,
    string UsuarioNome,
    string UsuarioEmail,
    Guid SetorId,
    string SetorNome,
    bool Habilitado);
