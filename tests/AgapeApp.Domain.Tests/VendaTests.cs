using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Tests;

public class VendaTests
{
    private static Venda CriarVendaValida() => Venda.Criar(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        30m,
        Guid.NewGuid(),
        "  Celular do caixa 2  ");

    [Fact]
    public void Criar_DeveGuardarDados_NormalizandoDispositivo()
    {
        var venda = CriarVendaValida();

        Assert.Equal(30m, venda.Total);
        Assert.Equal("Celular do caixa 2", venda.Dispositivo);
        Assert.NotEqual(Guid.Empty, venda.IdOperacaoCliente);
    }

    [Fact]
    public void Criar_SemDispositivo_DeveSerPermitido()
    {
        var venda = Venda.Criar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, Guid.NewGuid(), " ");

        Assert.Null(venda.Dispositivo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Criar_ComTotalInvalido_DeveLancarRegraNegocioException(decimal total)
    {
        Assert.Throws<RegraNegocioException>(() => Venda.Criar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), total, Guid.NewGuid(), null));
    }

    [Fact]
    public void Criar_SemIdOperacao_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() => Venda.Criar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, Guid.Empty, null));
    }
}

public class ItemVendaTests
{
    [Fact]
    public void Criar_DeveCalcularTotalDoItem()
    {
        var item = ItemVenda.Criar(Guid.NewGuid(), Guid.NewGuid(), "  Pastel  ", 7.50m, 3);

        Assert.Equal("Pastel", item.ProdutoNome);
        Assert.Equal(7.50m, item.PrecoUnitario);
        Assert.Equal(3, item.Quantidade);
        Assert.Equal(22.50m, item.Total);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_ComQuantidadeInvalida_DeveLancarRegraNegocioException(int quantidade)
    {
        Assert.Throws<RegraNegocioException>(() => ItemVenda.Criar(
            Guid.NewGuid(), Guid.NewGuid(), "Refrigerante", 5m, quantidade));
    }

    [Fact]
    public void Criar_ComPrecoInvalido_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() => ItemVenda.Criar(
            Guid.NewGuid(), Guid.NewGuid(), "Refrigerante", 0m, 1));
    }

    [Fact]
    public void Criar_SemNomeDoProduto_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() => ItemVenda.Criar(
            Guid.NewGuid(), Guid.NewGuid(), "  ", 5m, 1));
    }
}
