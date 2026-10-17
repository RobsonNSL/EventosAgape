using AgapeApp.Infrastructure.Security;

namespace AgapeApp.Infrastructure.Tests;

public class Argon2PasswordHashServiceTests
{
    [Fact]
    public void GerarHash_EVerificar_DevemSerCompativeis()
    {
        var servico = new Argon2PasswordHashService();

        var hash = servico.GerarHash("senha-secreta-123");

        Assert.NotEqual("senha-secreta-123", hash);
        Assert.True(servico.Verificar("senha-secreta-123", hash));
        Assert.False(servico.Verificar("senha-errada", hash));
    }

    [Fact]
    public void GerarHash_DeveProduzirHashesDiferentesParaAMesmaSenha()
    {
        var servico = new Argon2PasswordHashService();

        var primeiro = servico.GerarHash("senha-secreta-123");
        var segundo = servico.GerarHash("senha-secreta-123");

        // Salt aleatório: o mesmo texto nunca produz o mesmo hash.
        Assert.NotEqual(primeiro, segundo);
    }

    [Fact]
    public void Verificar_ComHashInvalido_DeveRetornarFalso()
    {
        var servico = new Argon2PasswordHashService();

        Assert.False(servico.Verificar("qualquer-senha", "isto-nao-e-um-hash"));
    }
}
