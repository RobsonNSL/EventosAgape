namespace AgapeApp.Mobile.Models;

// Contratos JSON da API (espelham os DTOs do AgapeApp.Application).
// Nomes idênticos aos do servidor para facilitar a manutenção lado a lado.

// ===== Autenticação =====
public record UsuarioAutenticadoDTO(Guid Id, string Nome, string Email, string Perfil, string? CaminhoFoto);

public record LoginResponse(
    string TokenAcesso,
    string TokenRenovacao,
    DateTimeOffset ExpiraEm,
    UsuarioAutenticadoDTO Usuario);

public record LoginRequest(string Email, string Senha, string? Dispositivo);

public record RenovarTokenRequest(string TokenRenovacao, string? Dispositivo);

public record SairRequest(string TokenRenovacao);

public record AlterarSenhaRequest(string SenhaAtual, string NovaSenha);

// ===== Eventos e setores =====
public record SetorDTO(Guid Id, Guid EventoId, string Nome);

public record MeuEventoDTO(
    Guid EventoId,
    string Nome,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string Situacao,
    Guid SetorId,
    string SetorNome,
    bool Habilitado);

public record EventoProdutoDTO(
    Guid Id,
    Guid EventoId,
    Guid ProdutoId,
    string ProdutoNome,
    decimal Preco,
    bool ControlaEstoque,
    int QuantidadeEstoque,
    string? ProdutoCaminhoImagem = null);

// ===== Cartões =====
public record CartaoSaldoDTO(
    Guid CartaoId,
    string Codigo,
    bool Ativo,
    Guid EventoId,
    decimal SaldoAtual);

// ===== Recarga =====
public record RecargaRequest(
    string CartaoCodigo,
    Guid EventoId,
    Guid SetorId,
    decimal Valor,
    Guid IdOperacaoCliente,
    string? Dispositivo);

public record RecargaResponse(
    Guid RecargaId,
    string CartaoCodigo,
    Guid EventoId,
    decimal Valor,
    decimal SaldoAtual,
    DateTimeOffset CriadoEm);

// ===== Venda =====
public record ItemVendaRequest(Guid EventoProdutoId, int Quantidade);

public record VendaRequest(
    string CartaoCodigo,
    Guid EventoId,
    Guid SetorId,
    IReadOnlyList<ItemVendaRequest> Itens,
    Guid IdOperacaoCliente,
    string? Dispositivo);

public record ItemVendaDTO(
    Guid EventoProdutoId,
    string ProdutoNome,
    decimal PrecoUnitario,
    int Quantidade,
    decimal Total);

public record VendaResponse(
    Guid VendaId,
    string CartaoCodigo,
    Guid EventoId,
    decimal Total,
    decimal SaldoAtual,
    IReadOnlyList<ItemVendaDTO> Itens,
    DateTimeOffset CriadoEm);
