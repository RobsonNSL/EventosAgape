using AgapeApp.Domain.Common;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Saldo de um cartão em um evento. Criado na primeira recarga; nunca atravessa eventos.
/// </summary>
public class SaldoCartaoEvento : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private SaldoCartaoEvento()
    {
    }

    public Guid CartaoId { get; private set; }

    public Guid EventoId { get; private set; }

    public decimal Saldo { get; private set; }

    /// <summary>
    /// Cria o saldo zerado do cartão no evento (primeira recarga).
    /// </summary>
    public static SaldoCartaoEvento Criar(Guid cartaoId, Guid eventoId) => new()
    {
        CartaoId = cartaoId,
        EventoId = eventoId,
        Saldo = 0m
    };

    /// <summary>
    /// Soma o valor ao saldo (recarga).
    /// </summary>
    public void Creditar(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor creditado deve ser maior que zero.");

        Saldo += valor;
        MarcarComoAtualizada();
    }

    /// <summary>
    /// Debita o valor do saldo (venda). Nunca permite saldo negativo (sem fiado).
    /// </summary>
    public void Debitar(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor debitado deve ser maior que zero.");

        if (Saldo < valor)
            throw new SaldoInsuficienteException(Saldo);

        Saldo -= valor;
        MarcarComoAtualizada();
    }
}
