using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Tests;

public class ProdutoTests
{
    [Fact]
    public void Criar_DeveNormalizarNome_EIniciarAtivo()
    {
        var produto = Produto.Criar("  Pastel de carne ", 8.50m);

        Assert.Equal("Pastel de carne", produto.Nome);
        Assert.Equal(8.50m, produto.PrecoPadrao);
        Assert.True(produto.Ativo);
        Assert.NotEqual(Guid.Empty, produto.Id);
    }

    [Fact]
    public void Criar_SemPrecoPadrao_DeveSerPermitido()
    {
        var produto = Produto.Criar("Refrigerante", null);

        Assert.Null(produto.PrecoPadrao);
    }

    [Fact]
    public void Criar_SemNome_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() => Produto.Criar(" ", 10m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_ComPrecoPadraoInvalido_DeveLancarRegraNegocioException(decimal preco)
    {
        Assert.Throws<RegraNegocioException>(() => Produto.Criar("Pastel", preco));
    }

    [Fact]
    public void Atualizar_DeveAlterarNomeEPreco()
    {
        var produto = Produto.Criar("Pastel", 8m);

        produto.Atualizar("Pastel de queijo", 9m);

        Assert.Equal("Pastel de queijo", produto.Nome);
        Assert.Equal(9m, produto.PrecoPadrao);
    }

    [Fact]
    public void Desativar_EAtivar_DevemAlterarSituacao()
    {
        var produto = Produto.Criar("Pastel", 8m);

        produto.Desativar();
        Assert.False(produto.Ativo);

        produto.Ativar();
        Assert.True(produto.Ativo);
    }
}
