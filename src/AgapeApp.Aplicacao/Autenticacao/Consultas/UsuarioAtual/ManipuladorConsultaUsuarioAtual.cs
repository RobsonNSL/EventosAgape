using AgapeApp.Aplicacao.Autenticacao.Dtos;
using AgapeApp.Aplicacao.Comum.Excecoes;
using AgapeApp.Aplicacao.Interfaces;
using MediatR;

namespace AgapeApp.Aplicacao.Autenticacao.Consultas.UsuarioAtual;

public class ManipuladorConsultaUsuarioAtual : IRequestHandler<ConsultaUsuarioAtual, UsuarioAutenticado>
{
    private readonly IRepositorioUsuarios _repositorioUsuarios;

    public ManipuladorConsultaUsuarioAtual(IRepositorioUsuarios repositorioUsuarios)
    {
        _repositorioUsuarios = repositorioUsuarios;
    }

    public async Task<UsuarioAutenticado> Handle(ConsultaUsuarioAtual consulta, CancellationToken cancellationToken)
    {
        var usuario = await _repositorioUsuarios.ObterPorIdAsync(consulta.UsuarioId, cancellationToken)
            ?? throw new ExcecaoNaoEncontrado("Usuário não encontrado.");

        return new UsuarioAutenticado(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil.ToString());
    }
}
