using AgapeApp.Application.Authentication.DTOs;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly SessionService _sessionService;

    public LoginCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHashService passwordHashService,
        SessionService sessionService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHashService = passwordHashService;
        _sessionService = sessionService;
    }

    public async Task<LoginResponse> Handle(LoginCommand comando, CancellationToken cancellationToken)
    {
        var emailNormalizado = Usuario.NormalizarEmail(comando.Email);

        var usuario = await _usuarioRepository.ObterPorEmailAsync(emailNormalizado, cancellationToken)
            ?? throw new AutenticacaoException(MensagensErro.CredenciaisInvalidas);

        if (!usuario.Ativo)
            throw new AutenticacaoException(MensagensErro.UsuarioDesativado);

        if (!_passwordHashService.Verificar(comando.Senha, usuario.SenhaHash))
            throw new AutenticacaoException(MensagensErro.CredenciaisInvalidas);

        return await _sessionService.EmitirAsync(usuario, comando.Dispositivo, cancellationToken);
    }
}
