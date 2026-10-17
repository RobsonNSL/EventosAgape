using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Exceptions;

/// <summary>
/// Lançada quando o saldo do cartão é insuficiente para a operação.
/// Carrega o saldo atual para a API devolver junto da mensagem (o app mostra o saldo).
/// </summary>
public class SaldoInsuficienteException : RegraNegocioException
{
    public SaldoInsuficienteException(decimal saldoAtual) : base(MensagensErro.SaldoInsuficiente)
    {
        SaldoAtual = saldoAtual;
    }

    public decimal SaldoAtual { get; }
}
