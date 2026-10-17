using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Eventos.DTOs;

/// <summary>
/// Conversão de <see cref="Evento"/> para <see cref="EventoDTO"/>.
/// </summary>
public static class EventoDTOExtensions
{
    public static EventoDTO ParaDTO(this Evento evento) => new(
        evento.Id,
        evento.Nome,
        evento.DataInicio,
        evento.DataFim,
        evento.Situacao.ToString(),
        evento.CriadoEm);
}
