using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Tests;

public class EventoProdutoTests
{
    private static EventoProduto CriarComEstoque(int estoqueInicial = 10) =>
        EventoProduto.Criar(Guid.NewGuid(), Guid.NewGuid(), 8.50m, true, estoqueInicial);

    [Fact]
    public void Criar_ComControleDeEstoque_DeveGuardarQuantidadeInicial()
    {
        var eventoProduto = CriarComEstoque(20);

        Assert.True(eventoProduto.ControlaEstoque);
        Assert.Equal(20, eventoProduto.QuantidadeEstoque);
        Assert.Equal(8.50m, eventoProduto.Preco);
    }

    [Fact]
    public void Criar_SemControleDeEstoque_DeveZerarQuantidade()
    {
        var eventoProduto = EventoProduto.Criar(Guid.NewGuid(), Guid.NewGuid(), 8m, false, 20);

        Assert.False(eventoProduto.ControlaEstoque);
        Assert.Equal(0, eventoProduto.QuantidadeEstoque);
    }

    [Fact]
    public void Criar_ComPrecoInvalido_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() =>
            EventoProduto.Criar(Guid.NewGuid(), Guid.NewGuid(), 0m, true, 10));
    }

    [Fact]
    public void Criar_ComEstoqueInicialNegativo_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() =>
            EventoProduto.Criar(Guid.NewGuid(), Guid.NewGuid(), 8m, true, -1));
    }

    [Fact]
    public void AjustarEstoque_SemControle_DeveLancarRegraNegocioException()
    {
        var eventoProduto = EventoProduto.Criar(Guid.NewGuid(), Guid.NewGuid(), 8m, false, 0);

        Assert.Throws<RegraNegocioException>(() => eventoProduto.AjustarEstoque(5));
    }

    [Fact]
    public void AjustarEstoque_ComZero_DeveLancarRegraNegocioException()
    {
        var eventoProduto = CriarComEstoque();

        Assert.Throws<RegraNegocioException>(() => eventoProduto.AjustarEstoque(0));
    }

    [Fact]
    public void AjustarEstoque_QueDeixariaNegativo_DeveLancarRegraNegocioException()
    {
        var eventoProduto = CriarComEstoque(5);

        Assert.Throws<RegraNegocioException>(() => eventoProduto.AjustarEstoque(-6));
    }

    [Fact]
    public void AjustarEstoque_DeveSomarAoSaldo()
    {
        var eventoProduto = CriarComEstoque(10);

        eventoProduto.AjustarEstoque(24);

        Assert.Equal(34, eventoProduto.QuantidadeEstoque);
    }

    [Fact]
    public void RetirarEstoque_ComQuantidadeSuficiente_DeveBaixar()
    {
        var eventoProduto = CriarComEstoque(10);

        eventoProduto.RetirarEstoque(4);

        Assert.Equal(6, eventoProduto.QuantidadeEstoque);
    }

    [Fact]
    public void RetirarEstoque_AcimaDoDisponivel_DeveLancarRegraNegocioException()
    {
        var eventoProduto = CriarComEstoque(3);

        Assert.Throws<RegraNegocioException>(() => eventoProduto.RetirarEstoque(4));
    }
}
