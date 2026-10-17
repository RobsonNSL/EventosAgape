using AgapeApp.Domain.Common;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Venda de produtos para um cartão em um evento. O total é sempre a soma dos itens.
/// O id_operacao_cliente garante a idempotência (retry ou toque duplo nunca duplica a venda).
/// </summary>
public class Venda : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private Venda()
    {
    }

    public Guid EventoId { get; private set; }

    public Guid CartaoId { get; private set; }

    public Guid SetorId { get; private set; }

    /// <summary>Operador que registrou a venda.</summary>
    public Guid UsuarioId { get; private set; }

    public decimal Total { get; private set; }

    /// <summary>Identificador único gerado pelo app para a idempotência.</summary>
    public Guid IdOperacaoCliente { get; private set; }

    public string? Dispositivo { get; private set; }

    public static Venda Criar(
        Guid eventoId,
        Guid cartaoId,
        Guid setorId,
        Guid usuarioId,
        decimal total,
        Guid idOperacaoCliente,
        string? dispositivo)
    {
        if (total <= 0)
            throw new RegraNegocioException(MensagensErro.VendaSemItens);

        if (idOperacaoCliente == Guid.Empty)
            throw new RegraNegocioException(MensagensErro.IdOperacaoObrigatoria);

        return new Venda
        {
            EventoId = eventoId,
            CartaoId = cartaoId,
            SetorId = setorId,
            UsuarioId = usuarioId,
            Total = total,
            IdOperacaoCliente = idOperacaoCliente,
            Dispositivo = string.IsNullOrWhiteSpace(dispositivo) ? null : dispositivo.Trim()
        };
    }
}
