using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.SetorProdutos.DTOs;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.SetorProdutos.Commands.VincularProdutoAoSetor;

public class VincularProdutoAoSetorCommandHandler
    : IRequestHandler<VincularProdutoAoSetorCommand, SetorProdutoDTO>
{
    private readonly ISetorRepository _setorRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IEventoProdutoRepository _eventoProdutoRepository;
    private readonly ISetorProdutoRepository _setorProdutoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VincularProdutoAoSetorCommandHandler(
        ISetorRepository setorRepository,
        IEventoRepository eventoRepository,
        IEventoProdutoRepository eventoProdutoRepository,
        ISetorProdutoRepository setorProdutoRepository,
        IUnitOfWork unitOfWork)
    {
        _setorRepository = setorRepository;
        _eventoRepository = eventoRepository;
        _eventoProdutoRepository = eventoProdutoRepository;
        _setorProdutoRepository = setorProdutoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SetorProdutoDTO> Handle(
        VincularProdutoAoSetorCommand comando,
        CancellationToken cancellationToken)
    {
        var setor = await _setorRepository.ObterPorIdAsync(comando.SetorId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.SetorNaoEncontrado);

        var evento = await _eventoRepository.ObterPorIdAsync(setor.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        var eventoProduto = await _eventoProdutoRepository
            .ObterPorIdAsync(comando.EventoProdutoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoProdutoNaoEncontrado);

        if (eventoProduto.EventoId != setor.EventoId)
            throw new RegraNegocioException(MensagensErro.ProdutoDeOutroEvento);

        if (await _setorProdutoRepository.ObterAsync(setor.Id, eventoProduto.Id, cancellationToken) is not null)
            throw new ConflitoException(MensagensErro.ProdutoJaVinculadoAoSetor);

        var vinculo = SetorProduto.Criar(setor.Id, eventoProduto.Id);

        _setorProdutoRepository.Adicionar(vinculo);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new SetorProdutoDTO(vinculo.Id, vinculo.SetorId, vinculo.EventoProdutoId);
    }
}
