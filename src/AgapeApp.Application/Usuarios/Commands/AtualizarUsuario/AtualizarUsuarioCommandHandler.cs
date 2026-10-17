using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Usuarios.DTOs;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Usuarios.Commands.AtualizarUsuario;

public class AtualizarUsuarioCommandHandler : IRequestHandler<AtualizarUsuarioCommand, UsuarioDTO>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UsuarioDTO> Handle(AtualizarUsuarioCommand comando, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        var emailNormalizado = Usuario.NormalizarEmail(comando.Email);

        if (!string.Equals(emailNormalizado, usuario.Email, StringComparison.Ordinal)
            && await _usuarioRepository.ExistePorEmailAsync(emailNormalizado, cancellationToken))
        {
            throw new ConflitoException(MensagensErro.UsuarioEmailDuplicado);
        }

        // Trocar o perfil do último administrador ativo deixaria o sistema sem ninguém para administrá-lo.
        if (usuario.EhAdministrador()
            && usuario.Ativo
            && comando.Perfil != PerfilUsuario.Administrador
            && !await _usuarioRepository.ExisteOutroAdministradorAtivoAsync(usuario.Id, cancellationToken))
        {
            throw new RegraNegocioException(MensagensErro.NaoPodeRemoverUltimoAdministrador);
        }

        usuario.Atualizar(comando.Nome, comando.Email, comando.Perfil);
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
