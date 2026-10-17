using AgapeApp.Dominio.Comum;
using AgapeApp.Dominio.Enumeracoes;
using AgapeApp.Dominio.Excecoes;

namespace AgapeApp.Dominio.Entidades;

/// <summary>
/// Usuário do sistema (administrador ou operador).
/// </summary>
public class Usuario : EntidadeBase
{
    // Construtor privado exigido pelo Entity Framework.
    private Usuario()
    {
    }

    public string Nome { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string SenhaHash { get; private set; } = null!;

    public PerfilUsuario Perfil { get; private set; }

    public bool Ativo { get; private set; }

    /// <summary>
    /// Cria um novo usuário ativo.
    /// </summary>
    public static Usuario Criar(string nome, string email, string senhaHash, PerfilUsuario perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ExcecaoRegraNegocio("O nome do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ExcecaoRegraNegocio("O e-mail do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ExcecaoRegraNegocio("O hash da senha é obrigatório.");

        return new Usuario
        {
            Nome = nome.Trim(),
            Email = NormalizarEmail(email),
            SenhaHash = senhaHash,
            Perfil = perfil,
            Ativo = true
        };
    }

    /// <summary>
    /// Normaliza o e-mail para comparação e armazenamento (sem espaços e em minúsculas).
    /// </summary>
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>
    /// Define um novo hash de senha para o usuário.
    /// </summary>
    public void AtualizarSenha(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ExcecaoRegraNegocio("O hash da senha é obrigatório.");

        SenhaHash = senhaHash;
        MarcarComoAtualizada();
    }

    public void Ativar()
    {
        Ativo = true;
        MarcarComoAtualizada();
    }

    public void Desativar()
    {
        Ativo = false;
        MarcarComoAtualizada();
    }

    public bool EhAdministrador() => Perfil == PerfilUsuario.Administrador;
}
