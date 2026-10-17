using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Usuarios.Commands.RedefinirSenhaUsuario;

public class RedefinirSenhaUsuarioCommandHandler : IRequestHandler<RedefinirSenhaUsuarioCommand, Unit>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ITokenRenovacaoRepository _tokenRenovacaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RedefinirSenhaUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHashService passwordHashService,
        ITokenRenovacaoRepository tokenRenovacaoRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHashService = passwordHashService;
        _tokenRenovacaoRepository = tokenRenovacaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        RedefinirSenhaUsuarioCommand comando,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        usuario.AtualizarSenha(_passwordHashService.GerarHash(comando.NovaSenha));

        // Quem tinha sessão aberta precisa entrar de novo com a senha nova.
        await _tokenRenovacaoRepository.RevogarTodosDoUsuarioAsync(usuario.Id, cancellationToken);

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Unit.Value;
    }
}
