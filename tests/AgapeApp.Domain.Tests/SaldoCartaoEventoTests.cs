using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Tests;

public class SaldoCartaoEventoTests
{
    [Fact]
    public void Criar_DeveIniciarZerado()
    {
        var saldo = SaldoCartaoEvento.Criar(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(0m, saldo.Saldo);
        Assert.NotEqual(Guid.Empty, saldo.Id);
    }

    [Fact]
    public void Creditar_DeveSomarAoSaldo()
    {
        var saldo = SaldoCartaoEvento.Criar(Guid.NewGuid(), Guid.NewGuid());

        saldo.Creditar(25.50m);
        saldo.Creditar(10m);

        Assert.Equal(35.50m, saldo.Saldo);
    }

    [Fact]
    public void Debitar_ComSaldoSuficiente_DeveBaixar()
    {
        var saldo = SaldoCartaoEvento.Criar(Guid.NewGuid(), Guid.NewGuid());
        saldo.Creditar(30m);

        saldo.Debitar(12.50m);

        Assert.Equal(17.50m, saldo.Saldo);
    }

    [Fact]
    public void Debitar_SemSaldo_DeveLancarSaldoInsuficienteComSaldoAtual()
    {
        var saldo = SaldoCartaoEvento.Criar(Guid.NewGuid(), Guid.NewGuid());
        saldo.Creditar(5m);

        var excecao = Assert.Throws<SaldoInsuficienteException>(() => saldo.Debitar(8m));

        Assert.Equal(5m, excecao.SaldoAtual);
    }
}
