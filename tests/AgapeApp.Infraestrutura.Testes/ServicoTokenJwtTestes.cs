using System.Text;
using AgapeApp.Dominio.Entidades;
using AgapeApp.Dominio.Enumeracoes;
using AgapeApp.Infraestrutura.Configuracoes;
using AgapeApp.Infraestrutura.Seguranca;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AgapeApp.Infraestrutura.Testes;

public class ServicoTokenJwtTestes
{
    private const string ChaveSecreta = "chave-de-teste-do-agapecapp-com-32-caracteres-ou-mais";
    private const string Emissor = "AgapeApp.Testes";
    private const string Audiencia = "AgapeApp.Testes";

    private static ServicoTokenJwt CriarServico()
        => new(Options.Create(new OpcoesAutenticacao
        {
            ChaveSecreta = ChaveSecreta,
            Emissor = Emissor,
            Audiencia = Audiencia,
            MinutosTokenAcesso = 30,
            DiasTokenRenovacao = 7
        }));

    [Fact]
    public async Task GerarTokenAcesso_DeveGerarTokenValidoComAsClaimsEsperadas()
    {
        var usuario = Usuario.Criar("Maria", "maria@exemplo.com", "hash", PerfilUsuario.Administrador);
        var servico = CriarServico();

        var (token, expiraEm) = servico.GerarTokenAcesso(usuario);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiraEm > DateTimeOffset.UtcNow);

        var resultado = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Emissor,
            ValidateAudience = true,
            ValidAudience = Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ChaveSecreta)),
            ValidateLifetime = true
        });

        Assert.True(resultado.IsValid);
        Assert.Equal(usuario.Id.ToString(), resultado.Claims["sub"].ToString());
        Assert.Equal("maria@exemplo.com", resultado.Claims["email"].ToString());
        Assert.Equal("Administrador", resultado.Claims["perfil"].ToString());
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
