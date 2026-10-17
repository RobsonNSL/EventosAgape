namespace AgapeApp.Application.Eventos.DTOs;

/// <summary>
/// Evento em que o operador está designado, com o setor dele no evento.
/// O operador só enxerga os eventos em que está designado.
/// </summary>
public record MeuEventoDTO(
    Guid EventoId,
    string Nome,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string Situacao,
    Guid SetorId,
    string SetorNome,
    bool Habilitado);
