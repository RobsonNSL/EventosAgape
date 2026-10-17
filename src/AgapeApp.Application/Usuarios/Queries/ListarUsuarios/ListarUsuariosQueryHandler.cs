using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Usuarios.DTOs;
using MediatR;

namespace AgapeApp.Application.Usuarios.Queries.ListarUsuarios;

public class ListarUsuariosQueryHandler : IRequestHandler<ListarUsuariosQuery, IReadOnlyList<UsuarioDTO>>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ListarUsuariosQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<IReadOnlyList<UsuarioDTO>> Handle(
        ListarUsuariosQuery consulta,
        CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioRepository.ListarAsync(cancellationToken);

        return usuarios
            .Select(usuario => new UsuarioDTO(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Perfil.ToString(),
                usuario.Ativo,
                usuario.CriadoEm))
            .ToList();
    }
}
