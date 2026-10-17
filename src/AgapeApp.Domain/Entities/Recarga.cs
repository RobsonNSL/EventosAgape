using AgapeApp.Domain.Common;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Recarga de cartão em um evento. O id_operacao_cliente garante a idempotência:
/// retry ou toque duplo do app nunca duplica a recarga.
/// </summary>
public class Recarga : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private Recarga()
    {
    }

    public Guid CartaoId { get; private set; }

    public Guid EventoId { get; private set; }

    public Guid SetorId { get; private set; }

    /// <summary>Operador que realizou a recarga.</summary>
    public Guid UsuarioId { get; private set; }

    public decimal Valor { get; private set; }

    /// <summary>Identificador único gerado pelo app para a idempotência.</summary>
    public Guid IdOperacaoCliente { get; private set; }

    public string? Dispositivo { get; private set; }

    public static Recarga Criar(
        Guid cartaoId,
        Guid eventoId,
        Guid setorId,
        Guid usuarioId,
        decimal valor,
        Guid idOperacaoCliente,
        string? dispositivo)
    {
        if (valor <= 0)
            throw new RegraNegocioException(MensagensErro.ValorInvalido);

        if (idOperacaoCliente == Guid.Empty)
            throw new RegraNegocioException(MensagensErro.IdOperacaoObrigatoria);

        return new Recarga
        {
            CartaoId = cartaoId,
            EventoId = eventoId,
            SetorId = setorId,
            UsuarioId = usuarioId,
            Valor = valor,
            IdOperacaoCliente = idOperacaoCliente,
            Dispositivo = string.IsNullOrWhiteSpace(dispositivo) ? null : dispositivo.Trim()
        };
    }
}
