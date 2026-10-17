namespace AgapeApp.Application.Recargas.DTOs;

/// <summary>
/// Dados de uma recarga para listagem/relatórios.
/// </summary>
public record RecargaDTO(
    Guid Id,
    string CartaoCodigo,
    decimal Valor,
    string UsuarioNome,
    string SetorNome,
    string? Dispositivo,
    DateTimeOffset CriadoEm);
