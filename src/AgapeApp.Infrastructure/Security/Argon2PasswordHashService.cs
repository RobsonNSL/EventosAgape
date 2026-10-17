using System.Security.Cryptography;
using System.Text;
using AgapeApp.Application.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace AgapeApp.Infrastructure.Security;

/// <summary>
/// Serviço de hash de senha com Argon2id.
/// O hash é armazenado no formato codificado (contém parâmetros + salt),
/// o que permite verificar sem guardar o salt separado.
/// </summary>
public class Argon2PasswordHashService : IPasswordHashService
{
    private const int CustoMemoriaKb = 65536;   // 64 MB
    private const int CustoProcessamento = 3;
    private const int Paralelismo = 4;
    private const int TamanhoHash = 32;
    private const int TamanhoSalt = 16;

    public string GerarHash(string senha)
    {
        var configuration = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing, // Argon2id
            Version = Argon2Version.Nineteen,
            TimeCost = CustoProcessamento,
            MemoryCost = CustoMemoriaKb,
            Lanes = Paralelismo,
            Threads = Paralelismo,
            Password = Encoding.UTF8.GetBytes(senha),
            Salt = RandomNumberGenerator.GetBytes(TamanhoSalt),
            HashLength = TamanhoHash
        };

        return Argon2.Hash(configuration);
    }

    public bool Verificar(string senha, string hash)
    {
        try
        {
            return Argon2.Verify(hash, Encoding.UTF8.GetBytes(senha));
        }
        catch
        {
            // Hash malformado ou incompatível: trata como senha inválida.
            return false;
        }
    }
}
