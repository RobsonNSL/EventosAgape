using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Usuarios.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.RemoverMinhaFoto;

public class RemoverMinhaFotoCommandHandler : IRequestHandler<RemoverMinhaFotoCommand, UsuarioDTO>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IArmazenamentoImagens _armazenamentoImagens;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public RemoverMinhaFotoCommandHandler(
        IUsuarioRepository usuarioRepository,
        IArmazenamentoImagens armazenamentoImagens,
        AuditoriaService auditoriaService,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _armazenamentoImagens = armazenamentoImagens;
        _auditoriaService = auditoriaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UsuarioDTO> Handle(RemoverMinhaFotoCommand comando, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterParaAtualizarAsync(comando.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        var fotoAnterior = usuario.CaminhoFoto;

        // Sem foto cadastrada não há nada a fazer (a remoção é idempotente).
        if (fotoAnterior is not null)
        {
            usuario.DefinirFoto(null);

            _auditoriaService.Registrar(
                usuario.Id,
                "RemoverFotoPerfil",
                "Usuario",
                usuario.Id);

            await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

            _armazenamentoImagens.Remover(fotoAnterior);
        }

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
