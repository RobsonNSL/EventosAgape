using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Tests;

public class UsuarioTests
{
    [Fact]
    public void Criar_DeveNormalizarEmail_EIniciarAtivo()
    {
        var usuario = Usuario.Criar("  Maria Silva ", "  MARIA@Exemplo.com ", "hash", PerfilUsuario.Operador);

        Assert.Equal("Maria Silva", usuario.Nome);
        Assert.Equal("maria@exemplo.com", usuario.Email);
        Assert.True(usuario.Ativo);
        Assert.Equal(PerfilUsuario.Operador, usuario.Perfil);
        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    [Fact]
    public void Criar_SemNome_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() =>
            Usuario.Criar(" ", "maria@exemplo.com", "hash", PerfilUsuario.Operador));
    }

    [Fact]
    public void Criar_SemEmail_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() =>
            Usuario.Criar("Maria", " ", "hash", PerfilUsuario.Operador));
    }

    [Fact]
    public void Desativar_EAtivar_DevemAlterarSituacao()
    {
        var usuario = Usuario.Criar("Maria", "maria@exemplo.com", "hash", PerfilUsuario.Operador);

        usuario.Desativar();
        Assert.False(usuario.Ativo);

        usuario.Ativar();
        Assert.True(usuario.Ativo);
    }
}
