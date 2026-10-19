using AgapeApp.Application.Common;
using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Produtos.DTOs;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.DefinirImagemProduto;

public class DefinirImagemProdutoCommandHandler : IRequestHandler<DefinirImagemProdutoCommand, ProdutoDTO>
{
    /// <summary>Subpasta das imagens de produto dentro da pasta pública de imagens.</summary>
    public const string Subpasta = "produtos";

    private readonly IProdutoRepository _produtoRepository;
    private readonly IArmazenamentoImagens _armazenamentoImagens;
    private readonly IUnitOfWork _unitOfWork;

    public DefinirImagemProdutoCommandHandler(
        IProdutoRepository produtoRepository,
        IArmazenamentoImagens armazenamentoImagens,
        IUnitOfWork unitOfWork)
    {
        _produtoRepository = produtoRepository;
        _armazenamentoImagens = armazenamentoImagens;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProdutoDTO> Handle(DefinirImagemProdutoCommand comando, CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.ObterParaAtualizarAsync(comando.ProdutoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.ProdutoNaoEncontrado);

        // Lê o arquivo em memória (limite de 5 MB): confere a assinatura antes de gravar.
        using var conteudo = new MemoryStream();
        await comando.Conteudo.CopyToAsync(conteudo, cancellationToken);

        var assinatura = new byte[FotoPerfil.BytesAssinatura];
        conteudo.Position = 0;
        var lidos = await conteudo.ReadAsync(assinatura, cancellationToken);

        if (!FotoPerfil.ConteudoEhImagem(assinatura.AsSpan(0, lidos)))
            throw new RegraNegocioException(MensagensErro.FotoConteudoInvalido);

        var imagemAnterior = produto.CaminhoImagem;

        conteudo.Position = 0;
        var caminho = await _armazenamentoImagens.SalvarAsync(
            conteudo,
            FotoPerfil.ExtensaoNormalizada(comando.NomeArquivo),
            Subpasta,
            cancellationToken);

        produto.DefinirImagem(caminho);

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        // A imagem anterior só sai depois de a nova estar persistida.
        _armazenamentoImagens.Remover(imagemAnterior);

        return produto.ParaDTO();
    }
}
