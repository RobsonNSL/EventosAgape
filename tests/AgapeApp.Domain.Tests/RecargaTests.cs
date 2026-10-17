using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Tests;

public class RecargaTests
{
    private static Recarga CriarRecargaValida() => Recarga.Criar(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        25m,
        Guid.NewGuid(),
        "  Celular do caixa 1  ");

    [Fact]
    public void Criar_DeveGuardarDados_NormalizandoDispositivo()
    {
        var recarga = CriarRecargaValida();

        Assert.Equal(25m, recarga.Valor);
        Assert.Equal("Celular do caixa 1", recarga.Dispositivo);
        Assert.NotEqual(Guid.Empty, recarga.IdOperacaoCliente);
    }

    [Fact]
    public void Criar_SemDispositivo_DeveSerPermitido()
    {
        var recarga = Recarga.Criar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, Guid.NewGuid(), " ");

        Assert.Null(recarga.Dispositivo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Criar_ComValorInvalido_DeveLancarRegraNegocioException(decimal valor)
    {
        Assert.Throws<RegraNegocioException>(() => Recarga.Criar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), valor, Guid.NewGuid(), null));
    }

    [Fact]
    public void Criar_SemIdOperacao_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() => Recarga.Criar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, Guid.Empty, null));
    }
}
