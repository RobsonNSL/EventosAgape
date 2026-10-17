namespace AgapeApp.Application.Cartoes.DTOs;

/// <summary>
/// Dados de um cartão para listagem/administração.
/// </summary>
public record CartaoDTO(
    Guid Id,
    string Codigo,
    bool Ativo,
    DateTimeOffset CriadoEm);
