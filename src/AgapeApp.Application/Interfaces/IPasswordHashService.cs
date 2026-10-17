namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Geração e verificação de hash de senha (Argon2id).
/// </summary>
public interface IPasswordHashService
{
    /// <summary>Gera o hash seguro de uma senha.</summary>
    string GerarHash(string senha);

    /// <summary>Verifica se a senha informada corresponde ao hash armazenado.</summary>
    bool Verificar(string senha, string hash);
}
