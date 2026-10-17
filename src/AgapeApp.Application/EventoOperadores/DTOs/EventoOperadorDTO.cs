namespace AgapeApp.Application.EventoOperadores.DTOs;

/// <summary>
/// Operador designado a um setor do evento.
/// </summary>
public record EventoOperadorDTO(
    Guid Id,
    Guid EventoId,
    Guid UsuarioId,
    string UsuarioNome,
    string UsuarioEmail,
    Guid SetorId,
    string SetorNome,
    bool Habilitado);
