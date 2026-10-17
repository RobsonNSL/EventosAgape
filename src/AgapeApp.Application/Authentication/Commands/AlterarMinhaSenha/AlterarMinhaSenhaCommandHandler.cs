using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.AlterarMinhaSenha;

public class AlterarMinhaSenhaCommandHandler : IRequestHandler<AlterarMinhaSenhaCommand, Unit>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ITokenRenovacaoRepository _tokenRenovacaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarMinhaSenhaCommandHandler(
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

    public async Task<Unit> Handle(AlterarMinhaSenhaCommand comando, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterParaAtualizarAsync(comando.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        if (!_passwordHashService.Verificar(comando.SenhaAtual, usuario.SenhaHash))
            throw new RegraNegocioException(MensagensErro.SenhaAtualIncorreta);

        usuario.AtualizarSenha(_passwordHashService.GerarHash(comando.NovaSenha));

        // As demais sessões em outros dispositivos deixam de renovar.
        await _tokenRenovacaoRepository.RevogarTodosDoUsuarioAsync(usuario.Id, cancellationToken);

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Unit.Value;
    }
}
