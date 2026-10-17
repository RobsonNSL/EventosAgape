using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Aplicacao.Usuarios.Dtos;
using MediatR;

namespace AgapeApp.Aplicacao.Usuarios.Consultas.ListarUsuarios;

public class ManipuladorConsultaListarUsuarios : IRequestHandler<ConsultaListarUsuarios, IReadOnlyList<UsuarioDetalhe>>
{
    private readonly IRepositorioUsuarios _repositorioUsuarios;

    public ManipuladorConsultaListarUsuarios(IRepositorioUsuarios repositorioUsuarios)
    {
        _repositorioUsuarios = repositorioUsuarios;
    }

    public async Task<IReadOnlyList<UsuarioDetalhe>> Handle(
        ConsultaListarUsuarios consulta,
        CancellationToken cancellationToken)
    {
        var usuarios = await _repositorioUsuarios.ListarAsync(cancellationToken);

        return usuarios
            .Select(usuario => new UsuarioDetalhe(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Perfil.ToString(),
                usuario.Ativo,
                usuario.CriadoEm))
            .ToList();
    }
}
