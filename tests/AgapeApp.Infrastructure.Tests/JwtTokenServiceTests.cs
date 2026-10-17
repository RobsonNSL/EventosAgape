using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using AgapeApp.Infrastructure.Options;
using AgapeApp.Infrastructure.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AgapeApp.Infrastructure.Tests;

public class JwtTokenServiceTests
{
    private const string ChaveSecreta = "chave-de-teste-do-agapecapp-com-32-caracteres-ou-mais";
    private const string Emissor = "AgapeApp.Tests";
    private const string Audiencia = "AgapeApp.Tests";

    // Qualificado de proposito: dentro de AgapeApp.Infrastructure.* o namespace `Options`
    // (da pasta Options) tem precedencia sobre a classe Microsoft.Extensions.Options.Options.
    private static JwtTokenService CriarServico(string chaveSecreta = ChaveSecreta)
        => new(Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions
        {
            ChaveSecreta = chaveSecreta,
            Emissor = Emissor,
            Audiencia = Audiencia,
            MinutosTokenAcesso = 30,
            DiasTokenRenovacao = 7
        }));

    private static TokenValidationParameters CriarParametrosDeValidacao(string chaveSecreta)
        => new()
        {
            ValidateIssuer = true,
            ValidIssuer = Emissor,
            ValidateAudience = true,
            ValidAudience = Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(JwtKeyDerivation.Derivar(chaveSecreta)),
            ValidateLifetime = true
        };

    [Fact]
    public async Task GerarTokenAcesso_DeveGerarTokenValidoComAsClaimsEsperadas()
    {
        var usuario = Usuario.Criar("Maria", "maria@exemplo.com", "hash", PerfilUsuario.Administrador);
        var servico = CriarServico();

        var (token, expiraEm) = servico.GerarTokenAcesso(usuario);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiraEm > DateTimeOffset.UtcNow);

        var resultado = await new JsonWebTokenHandler()
            .ValidateTokenAsync(token, CriarParametrosDeValidacao(ChaveSecreta));

        Assert.True(resultado.IsValid);
        Assert.Equal(usuario.Id.ToString(), resultado.Claims["sub"].ToString());
        Assert.Equal("maria@exemplo.com", resultado.Claims["email"].ToString());
        Assert.Equal("Administrador", resultado.Claims["perfil"].ToString());
    }

    [Fact]
    public async Task GerarTokenAcesso_ComChaveCurta_DeveFuncionar()
    {
        // "secretHash" tem menos de 16 bytes, mas a derivação SHA-256 garante o mínimo exigido pelo HS256.
        var usuario = Usuario.Criar("Maria", "maria@exemplo.com", "hash", PerfilUsuario.Operador);
        var servico = CriarServico("secretHash");

        var (token, _) = servico.GerarTokenAcesso(usuario);

        var resultado = await new JsonWebTokenHandler()
            .ValidateTokenAsync(token, CriarParametrosDeValidacao("secretHash"));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void GerarTokenRenovacao_DeveSerAleatorioEUnicoPorHash()
    {
        var servico = CriarServico();

        var primeiro = servico.GerarTokenRenovacao();
        var segundo = servico.GerarTokenRenovacao();

        Assert.False(string.IsNullOrWhiteSpace(primeiro));
        Assert.NotEqual(primeiro, segundo);

        // O hash é determinístico: o mesmo token sempre gera o mesmo hash.
        Assert.Equal(
            servico.CalcularHashTokenRenovacao(primeiro),
            servico.CalcularHashTokenRenovacao(primeiro));

        // Tokens diferentes geram hashes diferentes.
        Assert.NotEqual(
            servico.CalcularHashTokenRenovacao(primeiro),
            servico.CalcularHashTokenRenovacao(segundo));
    }
}
