using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Usuarios.DTOs;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Usuarios.Commands.CriarUsuario;

public class CriarUsuarioCommandHandler : IRequestHandler<CriarUsuarioCommand, UsuarioDTO>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IUnitOfWork _unitOfWork;

    public CriarUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHashService passwordHashService,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHashService = passwordHashService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UsuarioDTO> Handle(CriarUsuarioCommand comando, CancellationToken cancellationToken)
    {
        var emailNormalizado = Usuario.NormalizarEmail(comando.Email);

        if (await _usuarioRepository.ExistePorEmailAsync(emailNormalizado, cancellationToken))
            throw new ConflitoException(MensagensErro.UsuarioEmailDuplicado);

        var usuario = Usuario.Criar(
            comando.Nome,
            emailNormalizado,
            _passwordHashService.GerarHash(comando.Senha),
            comando.Perfil);

        _usuarioRepository.Adicionar(usuario);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new UsuarioDTO(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil.ToString(),
            usuario.Ativo,
            usuario.CriadoEm);
    }
}
