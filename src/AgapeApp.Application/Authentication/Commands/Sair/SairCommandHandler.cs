using AgapeApp.Application.Interfaces;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.Sair;

public class SairCommandHandler : IRequestHandler<SairCommand, Unit>
{
    private readonly ITokenRenovacaoRepository _tokenRenovacaoRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public SairCommandHandler(
        ITokenRenovacaoRepository tokenRenovacaoRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _tokenRenovacaoRepository = tokenRenovacaoRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SairCommand comando, CancellationToken cancellationToken)
    {
        var hash = _tokenService.CalcularHashTokenRenovacao(comando.TokenRenovacao);

        var registro = await _tokenRenovacaoRepository.ObterPorHashAsync(hash, cancellationToken);

        if (registro is not null && registro.EstaValido(DateTimeOffset.UtcNow))
        {
            registro.Revogar();
            await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
