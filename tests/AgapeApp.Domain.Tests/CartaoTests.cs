using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Tests;

public class CartaoTests
{
    [Fact]
    public void Criar_DeveNormalizarCodigoParaMaiusculas_EIniciarAtivo()
    {
        var cartao = Cartao.Criar("  card-0001 ");

        Assert.Equal("CARD-0001", cartao.Codigo);
        Assert.True(cartao.Ativo);
        Assert.NotEqual(Guid.Empty, cartao.Id);
    }

    [Fact]
    public void Criar_SemCodigo_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() => Cartao.Criar(" "));
    }

    [Fact]
    public void Desativar_EAtivar_DevemAlterarSituacao()
    {
        var cartao = Cartao.Criar("CARD-0002");

        cartao.Desativar();
        Assert.False(cartao.Ativo);

        cartao.Ativar();
        Assert.True(cartao.Ativo);
    }
}
