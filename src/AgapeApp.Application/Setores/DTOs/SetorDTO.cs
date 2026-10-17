namespace AgapeApp.Application.Setores.DTOs;

/// <summary>
/// Dados de um setor do evento.
/// </summary>
public record SetorDTO(
    Guid Id,
    Guid EventoId,
    string Nome);
