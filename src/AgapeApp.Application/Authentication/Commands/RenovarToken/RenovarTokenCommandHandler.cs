using AgapeApp.Application.Authentication.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.RenovarToken;

public class RenovarTokenCommandHandler : IRequestHandler<RenovarTokenCommand, LoginResponse>
{
    private readonly ITokenRenovacaoRepository _tokenRenovacaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;
    private readonly SessionService _sessionService;

    public RenovarTokenCommandHandler(
        ITokenRenovacaoRepository tokenRenovacaoRepository,
        IUsuarioRepository usuarioRepository,
        ITokenService tokenService,
        SessionService sessionService)
    {
        _tokenRenovacaoRepository = tokenRenovacaoRepository;
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
        _sessionService = sessionService;
    }

    public async Task<LoginResponse> Handle(RenovarTokenCommand comando, CancellationToken cancellationToken)
    {
        var hash = _tokenService.CalcularHashTokenRenovacao(comando.TokenRenovacao);

        var registro = await _tokenRenovacaoRepository.ObterPorHashAsync(hash, cancellationToken)
            ?? throw new AutenticacaoException(MensagensErro.SessaoInvalida);

        if (!registro.EstaValido(DateTimeOffset.UtcNow))
            throw new AutenticacaoException(MensagensErro.SessaoExpirada);

        var usuario = await _usuarioRepository.ObterPorIdAsync(registro.UsuarioId, cancellationToken)
            ?? throw new AutenticacaoException(MensagensErro.SessaoInvalida);

        if (!usuario.Ativo)
            throw new AutenticacaoException(MensagensErro.UsuarioDesativado);

        // Rotação: o token antigo é revogado e um novo par é emitido.
        registro.Revogar();

        return await _sessionService.EmitirAsync(usuario, comando.Dispositivo, cancellationToken);
    }
}
