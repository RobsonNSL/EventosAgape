using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Setores.DTOs;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Setores.Commands.CriarSetor;

public class CriarSetorCommandHandler : IRequestHandler<CriarSetorCommand, SetorDTO>
{
    private readonly IEventoRepository _eventoRepository;
    private readonly ISetorRepository _setorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarSetorCommandHandler(
        IEventoRepository eventoRepository,
        ISetorRepository setorRepository,
        IUnitOfWork unitOfWork)
    {
        _eventoRepository = eventoRepository;
        _setorRepository = setorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SetorDTO> Handle(CriarSetorCommand comando, CancellationToken cancellationToken)
    {
        var evento = await _eventoRepository.ObterPorIdAsync(comando.EventoId, cancellationToken)
            ?? throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        evento.GarantirNaoEncerrado();

        if (await _setorRepository.ExisteNoEventoAsync(comando.EventoId, comando.Nome, null, cancellationToken))
            throw new ConflitoException(MensagensErro.SetorJaCadastrado);

        var setor = Setor.Criar(comando.EventoId, comando.Nome);

        _setorRepository.Adicionar(setor);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new SetorDTO(setor.Id, setor.EventoId, setor.Nome);
    }
}
