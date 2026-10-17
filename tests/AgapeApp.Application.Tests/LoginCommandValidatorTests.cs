using AgapeApp.Application.Authentication.Commands.Login;

namespace AgapeApp.Application.Tests;

public class LoginCommandValidatorTests
{
    [Fact]
    public void ComandoInvalido_DeveRetornarErrosDeEmailESenha()
    {
        var validador = new LoginCommandValidator();

        var resultado = validador.Validate(new LoginCommand("", ""));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(LoginCommand.Email));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(LoginCommand.Senha));
    }

    [Fact]
    public void EmailInvalido_DeveRetornarErroDeFormato()
    {
        var validador = new LoginCommandValidator();

        var resultado = validador.Validate(new LoginCommand("nao-e-email", "senha123"));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void ComandoValido_NaoDeveRetornarErros()
    {
        var validador = new LoginCommandValidator();

        var resultado = validador.Validate(new LoginCommand("maria@exemplo.com", "senha123"));

        Assert.True(resultado.IsValid);
    }
}
