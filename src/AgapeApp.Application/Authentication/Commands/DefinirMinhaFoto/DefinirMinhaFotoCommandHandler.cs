using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Common;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Usuarios.DTOs;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.DefinirMinhaFoto;

public class DefinirMinhaFotoCommandHandler : IRequestHandler<DefinirMinhaFotoCommand, UsuarioDTO>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IArmazenamentoImagens _armazenamentoImagens;
    private readonly AuditoriaService _auditoriaService;
    private readonly IUnitOfWork _unitOfWork;

    public DefinirMinhaFotoCommandHandler(
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

    public async Task<UsuarioDTO> Handle(DefinirMinhaFotoCommand comando, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterParaAtualizarAsync(comando.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        // Lê o arquivo em memória (limite de 5 MB): confere a assinatura antes de gravar.
        using var conteudo = new MemoryStream();
        await comando.Conteudo.CopyToAsync(conteudo, cancellationToken);

        var assinatura = new byte[FotoPerfil.BytesAssinatura];
        conteudo.Position = 0;
        var lidos = await conteudo.ReadAsync(assinatura, cancellationToken);

        if (!FotoPerfil.ConteudoEhImagem(assinatura.AsSpan(0, lidos)))
            throw new RegraNegocioException(MensagensErro.FotoConteudoInvalido);

        var fotoAnterior = usuario.CaminhoFoto;

        conteudo.Position = 0;
        var caminho = await _armazenamentoImagens.SalvarAsync(
            conteudo,
            FotoPerfil.ExtensaoNormalizada(comando.NomeArquivo),
            cancellationToken: cancellationToken);

        usuario.DefinirFoto(caminho);

        _auditoriaService.Registrar(
            usuario.Id,
            "DefinirFotoPerfil",
            "Usuario",
            usuario.Id,
            dados: new { caminho });

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        // A foto anterior só sai depois de a nova estar persistida.
        _armazenamentoImagens.Remover(fotoAnterior);

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
