using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Dominio.Entidades;
using AgapeApp.Dominio.Enumeracoes;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infraestrutura.Banco.Repositorios;

public class RepositorioUsuarios : IRepositorioUsuarios
{
    private readonly ContextoAgape _contexto;

    public RepositorioUsuarios(ContextoAgape contexto)
    {
        _contexto = contexto;
    }

    public void Adicionar(Usuario usuario) => _contexto.Usuarios.Add(usuario);

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _contexto.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => _contexto.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Email == email, cancellationToken);

    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => _contexto.Usuarios
            .AsNoTracking()
            .AnyAsync(usuario => usuario.Email == email, cancellationToken);

    public Task<bool> ExisteAlgumAdministradorAsync(CancellationToken cancellationToken = default)
        => _contexto.Usuarios
            .AsNoTracking()
            .AnyAsync(usuario => usuario.Perfil == PerfilUsuario.Administrador, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default)
        => await _contexto.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync(cancellationToken);
}
