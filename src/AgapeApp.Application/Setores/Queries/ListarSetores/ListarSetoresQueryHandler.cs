using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Setores.DTOs;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Setores.Queries.ListarSetores;

public class ListarSetoresQueryHandler : IRequestHandler<ListarSetoresQuery, IReadOnlyList<SetorDTO>>
{
    private readonly ISetorRepository _setorRepository;
    private readonly IEventoRepository _eventoRepository;

    public ListarSetoresQueryHandler(
        ISetorRepository setorRepository,
        IEventoRepository eventoRepository)
    {
        _setorRepository = setorRepository;
        _eventoRepository = eventoRepository;
    }

    public async Task<IReadOnlyList<SetorDTO>> Handle(
        ListarSetoresQuery consulta,
        CancellationToken cancellationToken)
    {
        if (await _eventoRepository.ObterPorIdAsync(consulta.EventoId, cancellationToken) is null)
            throw new NaoEncontradoException(MensagensErro.EventoNaoEncontrado);

        var setores = await _setorRepository.ListarPorEventoAsync(consulta.EventoId, cancellationToken);

        return setores
            .Select(setor => new SetorDTO(setor.Id, setor.EventoId, setor.Nome))
            .ToList();
    }
}
