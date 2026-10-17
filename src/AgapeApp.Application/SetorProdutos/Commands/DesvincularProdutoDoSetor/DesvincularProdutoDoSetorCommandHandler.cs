using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.SetorProdutos.Commands.DesvincularProdutoDoSetor;

public class DesvincularProdutoDoSetorCommandHandler : IRequestHandler<DesvincularProdutoDoSetorCommand>
{
    private readonly ISetorRepository _setorRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly ISetorProdutoRepository _setorProdutoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DesvincularProdutoDoSetorCommandHandler(
        ISetorRepository setorRepository,
        IEventoRepository eventoRepository,
        ISetorProdutoRepository setorProdutoRepository,
        IUnitOfWork unitOfWork)
    {
        _setorRepository = setorRepository;
        _eventoRepository = eventoRepository;
        _setorProdutoRepository = setorProdutoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DesvincularProdutoDoSetorCommand comando, CancellationToken cancellationToken)
    {
        var setor = await _setorRepository.ObterPorIdAsync(comando.SetorId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.SetorNaoEncontrado);

        var evento = await _eventoRepository.ObterPorIdAsync(setor.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        var vinculo = await _setorProdutoRepository
            .ObterAsync(comando.SetorId, comando.EventoProdutoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.VinculoNaoEncontrado);

        _setorProdutoRepository.Remover(vinculo);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);
    }
}
