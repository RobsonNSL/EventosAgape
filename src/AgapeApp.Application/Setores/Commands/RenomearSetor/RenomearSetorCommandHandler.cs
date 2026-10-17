using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Setores.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Setores.Commands.RenomearSetor;

public class RenomearSetorCommandHandler : IRequestHandler<RenomearSetorCommand, SetorDTO>
{
    private readonly ISetorRepository _setorRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RenomearSetorCommandHandler(
        ISetorRepository setorRepository,
        IEventoRepository eventoRepository,
        IUnitOfWork unitOfWork)
    {
        _setorRepository = setorRepository;
        _eventoRepository = eventoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SetorDTO> Handle(RenomearSetorCommand comando, CancellationToken cancellationToken)
    {
        var setor = await _setorRepository.ObterParaAtualizarAsync(comando.Id, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.SetorNaoEncontrado);

        var evento = await _eventoRepository.ObterPorIdAsync(setor.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        if (await _setorRepository.ExisteNoEventoAsync(setor.EventoId, comando.Nome, setor.Id, cancellationToken))
            throw new ConflitoException(MensagensErro.SetorJaCadastrado);

        setor.Renomear(comando.Nome);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new SetorDTO(setor.Id, setor.EventoId, setor.Nome);
    }
}
