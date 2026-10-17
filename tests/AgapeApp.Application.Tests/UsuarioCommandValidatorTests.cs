using AgapeApp.Application.Authentication.Commands.AlterarMinhaSenha;
using AgapeApp.Application.Usuarios.Commands.AtualizarUsuario;
using AgapeApp.Application.Usuarios.Commands.RedefinirSenhaUsuario;
using AgapeApp.Domain.Enums;

namespace AgapeApp.Application.Tests;

public class UsuarioCommandValidatorTests
{
    [Fact]
    public void AtualizarUsuario_ComandoInvalido_DeveRetornarErrosDeNomeEmailEPerfil()
    {
        var validador = new AtualizarUsuarioCommandValidator();

        var resultado = validador.Validate(new AtualizarUsuarioCommand(
            Guid.NewGuid(),
            "",
            "nao-e-email",
            (PerfilUsuario)99));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(AtualizarUsuarioCommand.Nome));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(AtualizarUsuarioCommand.Email));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(AtualizarUsuarioCommand.Perfil));
    }

    [Fact]
    public void AtualizarUsuario_ComandoValido_NaoDeveRetornarErros()
    {
        var validador = new AtualizarUsuarioCommandValidator();

        var resultado = validador.Validate(new AtualizarUsuarioCommand(
            Guid.NewGuid(),
            "Maria Souza",
            "maria@exemplo.com",
            PerfilUsuario.Operador));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void RedefinirSenha_ComSenhaCurta_DeveRetornarErroDeTamanhoMinimo()
    {
        var validador = new RedefinirSenhaUsuarioCommandValidator();

        var resultado = validador.Validate(new RedefinirSenhaUsuarioCommand(Guid.NewGuid(), "123"));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(RedefinirSenhaUsuarioCommand.NovaSenha));
    }

    [Fact]
    public void AlterarMinhaSenha_SemSenhaAtual_DeveRetornarErroDeObrigatoriedade()
    {
        var validador = new AlterarMinhaSenhaCommandValidator();

        var resultado = validador.Validate(new AlterarMinhaSenhaCommand(Guid.NewGuid(), "", "NovaSenha@123"));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(AlterarMinhaSenhaCommand.SenhaAtual));
        Assert.DoesNotContain(resultado.Errors, erro => erro.PropertyName == nameof(AlterarMinhaSenhaCommand.NovaSenha));
    }
}
