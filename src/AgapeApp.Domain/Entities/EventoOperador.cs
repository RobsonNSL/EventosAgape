using AgapeApp.Domain.Common;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Designação de um operador a um setor do evento.
/// O operador só enxerga os eventos em que está designado.
/// </summary>
public class EventoOperador : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private EventoOperador()
    {
    }

    public Guid EventoId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public Guid SetorId { get; private set; }

    public bool Habilitado { get; private set; }

    /// <summary>
    /// Designa o operador ao setor (habilitado por padrão; o admin pode desabilitar depois).
    /// </summary>
    public static EventoOperador Criar(Guid eventoId, Guid usuarioId, Guid setorId) => new()
    {
        EventoId = eventoId,
        UsuarioId = usuarioId,
        SetorId = setorId,
        Habilitado = true
    };

    /// <summary>
    /// Troca o setor designado e/ou altera a habilitação do operador no evento.
    /// </summary>
    public void Atualizar(Guid setorId, bool habilitado)
    {
        SetorId = setorId;
        Habilitado = habilitado;
        MarcarComoAtualizada();
    }
}
