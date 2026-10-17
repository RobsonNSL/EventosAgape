using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso aos dados de usuários.
/// </summary>
public interface IUsuarioRepository
{
    void Adicionar(Usuario usuario);

    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExisteAlgumAdministradorAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default);
}
