using AgapeApp.Domain.Common;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Usuário do sistema (administrador ou operador).
/// </summary>
public class Usuario : BaseEntity
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
            throw new RegraNegocioException(MensagensErro.NomeUsuarioObrigatorio);

        if (string.IsNullOrWhiteSpace(email))
            throw new RegraNegocioException(MensagensErro.EmailUsuarioObrigatorio);

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new RegraNegocioException(MensagensErro.HashSenhaObrigatorio);

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
    /// Atualiza os dados cadastrais do usuário (nome, e-mail e perfil).
    /// A situação (ativo/inativo) e a senha são alteradas pelos próprios métodos.
    /// </summary>
    public void Atualizar(string nome, string email, PerfilUsuario perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException(MensagensErro.NomeUsuarioObrigatorio);

        if (string.IsNullOrWhiteSpace(email))
            throw new RegraNegocioException(MensagensErro.EmailUsuarioObrigatorio);

        Nome = nome.Trim();
        Email = NormalizarEmail(email);
        Perfil = perfil;
        MarcarComoAtualizada();
    }

    /// <summary>
    /// Define um novo hash de senha para o usuário.
    /// </summary>
    public void AtualizarSenha(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new RegraNegocioException(MensagensErro.HashSenhaObrigatorio);

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
