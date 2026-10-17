using System.Security.Cryptography;
using System.Text;
using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Dominio.Entidades;
using AgapeApp.Infraestrutura.Configuracoes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AgapeApp.Infraestrutura.Seguranca;

/// <summary>
/// Geração de tokens JWT (acesso) e de tokens aleatórios de renovação.
/// </summary>
public class ServicoTokenJwt : IServicoToken
{
    private readonly OpcoesAutenticacao _opcoes;

    public ServicoTokenJwt(IOptions<OpcoesAutenticacao> opcoes)
    {
        _opcoes = opcoes.Value;
    }

    public (string Token, DateTimeOffset ExpiraEm) GerarTokenAcesso(Usuario usuario)
    {
        var agora = DateTimeOffset.UtcNow;
        var expiraEm = agora.AddMinutes(_opcoes.MinutosTokenAcesso);

        var descritor = new SecurityTokenDescriptor
        {
            Issuer = _opcoes.Emissor,
            Audience = _opcoes.Audiencia,
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
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opcoes.ChaveSecreta)),
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
        => DateTimeOffset.UtcNow.AddDays(_opcoes.DiasTokenRenovacao);
}
