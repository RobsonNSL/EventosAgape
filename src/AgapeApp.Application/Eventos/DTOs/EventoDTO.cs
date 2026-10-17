namespace AgapeApp.Application.Eventos.DTOs;

/// <summary>
/// Dados de um evento para listagem/administração.
/// </summary>
public record EventoDTO(
    Guid Id,
    string Nome,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string Situacao,
    DateTimeOffset CriadoEm);
