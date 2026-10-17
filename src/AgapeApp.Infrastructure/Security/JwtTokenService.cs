using System.Security.Cryptography;
using System.Text;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AgapeApp.Infrastructure.Security;

/// <summary>
/// Geração de tokens JWT (acesso) e de tokens aleatórios de renovação.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly AuthenticationOptions _options;

    public JwtTokenService(IOptions<AuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTimeOffset ExpiraEm) GerarTokenAcesso(Usuario usuario)
    {
        var agora = DateTimeOffset.UtcNow;
        var expiraEm = agora.AddMinutes(_options.MinutosTokenAcesso);

        var descritor = new SecurityTokenDescriptor
        {
            Issuer = _options.Emissor,
            Audience = _options.Audiencia,
            IssuedAt = agora.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = usuario.Id.ToString(),
                ["nome"] = usuario.Nome,
                ["email"] = usuario.Email,
                ["perfil"] = usuario.Perfil.ToString()
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(JwtKeyDerivation.Derivar(_options.ChaveSecreta)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descritor);

        return (token, expiraEm);
    }

    public string GerarTokenRenovacao()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string CalcularHashTokenRenovacao(string token)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public DateTimeOffset ObterExpiracaoTokenRenovacao()
        => DateTimeOffset.UtcNow.AddDays(_options.DiasTokenRenovacao);
}
