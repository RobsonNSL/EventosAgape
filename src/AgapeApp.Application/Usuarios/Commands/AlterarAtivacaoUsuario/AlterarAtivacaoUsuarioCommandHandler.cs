using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Usuarios.DTOs;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Usuarios.Commands.AlterarAtivacaoUsuario;

public class AlterarAtivacaoUsuarioCommandHandler
    : IRequestHandler<AlterarAtivacaoUsuarioCommand, UsuarioDTO>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenRenovacaoRepository _tokenRenovacaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarAtivacaoUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        ITokenRenovacaoRepository tokenRenovacaoRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _tokenRenovacaoRepository = tokenRenovacaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UsuarioDTO> Handle(
        AlterarAtivacaoUsuarioCommand comando,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        if (comando.Ativo)
        {
            usuario.Ativar();
        }
        else
        {
            if (usuario.Id == comando.SolicitanteId)
                throw new RegraNegocioException(MensagensErro.NaoPodeDesativarProprioUsuario);

            if (usuario.EhAdministrador()
                && usuario.Ativo
                && !await _usuarioRepository.ExisteOutroAdministradorAtivoAsync(usuario.Id, cancellationToken))
            {
                throw new RegraNegocioException(MensagensErro.NaoPodeRemoverUltimoAdministrador);
            }

            usuario.Desativar();

            // As sessões abertas do usuário deixam de renovar imediatamente.
            await _tokenRenovacaoRepository.RevogarTodosDoUsuarioAsync(usuario.Id, cancellationToken);
        }

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new UsuarioDTO(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil.ToString(),
            usuario.Ativo,
            usuario.CriadoEm,
            usuario.CaminhoFoto);
    }
}
