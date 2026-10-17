using AgapeApp.Application.Authentication.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Authentication.Queries.UsuarioAtual;

public class ObterUsuarioAtualQueryHandler : IRequestHandler<ObterUsuarioAtualQuery, UsuarioAutenticadoDTO>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObterUsuarioAtualQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<UsuarioAutenticadoDTO> Handle(ObterUsuarioAtualQuery consulta, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(consulta.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        return new UsuarioAutenticadoDTO(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil.ToString(), usuario.CaminhoFoto);
    }
}
