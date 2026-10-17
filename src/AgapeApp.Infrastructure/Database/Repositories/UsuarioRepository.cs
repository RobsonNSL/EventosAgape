using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AgapeDbContext _context;

    public UsuarioRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(Usuario usuario) => _context.Usuarios.Add(usuario);

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);

    // Sem AsNoTracking: edição, ativação/desativação e redefinição de senha alteram a entidade.
    public Task<Usuario?> ObterParaAtualizarAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Email == email, cancellationToken);

    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => _context.Usuarios
            .AsNoTracking()
            .AnyAsync(usuario => usuario.Email == email, cancellationToken);

    public Task<bool> ExisteAlgumAdministradorAsync(CancellationToken cancellationToken = default)
        => _context.Usuarios
            .AsNoTracking()
            .AnyAsync(usuario => usuario.Perfil == PerfilUsuario.Administrador, cancellationToken);

    public Task<bool> ExisteOutroAdministradorAtivoAsync(
        Guid ignorarUsuarioId,
        CancellationToken cancellationToken = default)
        => _context.Usuarios
            .AsNoTracking()
            .AnyAsync(
                usuario => usuario.Perfil == PerfilUsuario.Administrador
                    && usuario.Ativo
                    && usuario.Id != ignorarUsuarioId,
                cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default)
        => await _context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ObterPorIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();

        if (lista.Count == 0)
            return [];

        return await _context.Usuarios
            .AsNoTracking()
            .Where(usuario => lista.Contains(usuario.Id))
            .ToListAsync(cancellationToken);
    }
}
