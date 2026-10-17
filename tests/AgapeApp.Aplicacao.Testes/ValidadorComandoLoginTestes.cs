using AgapeApp.Aplicacao.Autenticacao.Comandos.Login;

namespace AgapeApp.Aplicacao.Testes;

public class ValidadorComandoLoginTestes
{
    [Fact]
    public void ComandoInvalido_DeveRetornarErrosDeEmailESenha()
    {
        var validador = new ValidadorComandoLogin();

        var resultado = validador.Validate(new ComandoLogin("", ""));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(ComandoLogin.Email));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(ComandoLogin.Senha));
    }

    [Fact]
    public void EmailInvalido_DeveRetornarErroDeFormato()
    {
        var validador = new ValidadorComandoLogin();

        var resultado = validador.Validate(new ComandoLogin("nao-e-email", "senha123"));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.PropertyName == nameof(ComandoLogin.Email));
    }

    [Fact]
    public void ComandoValido_NaoDeveRetornarErros()
    {
        var validador = new ValidadorComandoLogin();

        var resultado = validador.Validate(new ComandoLogin("maria@exemplo.com", "senha123"));

        Assert.True(resultado.IsValid);
    }
}
