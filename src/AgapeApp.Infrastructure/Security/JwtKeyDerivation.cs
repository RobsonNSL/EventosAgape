using System.Security.Cryptography;
using System.Text;

namespace AgapeApp.Infrastructure.Security;

/// <summary>
/// Deriva a chave de assinatura dos tokens JWT a partir de "Autenticacao:ChaveSecreta".
/// O algoritmo HS256 exige uma chave de no mínimo 128 bits (16 bytes), então o valor
/// configurado é transformado em uma chave de 256 bits via SHA-256 — assim qualquer
/// tamanho de segredo funciona (o ideal continua sendo um segredo longo e aleatório).
/// </summary>
public static class JwtKeyDerivation
{
    public static byte[] Derivar(string chaveSecreta)
        => SHA256.HashData(Encoding.UTF8.GetBytes(chaveSecreta));
}
