using AgapeApp.Aplicacao.Interfaces;
using MediatR;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.Sair;

public class ManipuladorComandoSair : IRequestHandler<ComandoSair, Unit>
{
    private readonly IRepositorioTokensRenovacao _repositorioTokensRenovacao;
    private readonly IServicoToken _servicoToken;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public ManipuladorComandoSair(
        IRepositorioTokensRenovacao repositorioTokensRenovacao,
        IServicoToken servicoToken,
        IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _repositorioTokensRenovacao = repositorioTokensRenovacao;
        _servicoToken = servicoToken;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    public async Task<Unit> Handle(ComandoSair comando, CancellationToken cancellationToken)
    {
        var hash = _servicoToken.CalcularHashTokenRenovacao(comando.TokenRenovacao);

        var registro = await _repositorioTokensRenovacao.ObterPorHashAsync(hash, cancellationToken);

        if (registro is not null && registro.EstaValido(DateTimeOffset.UtcNow))
        {
            registro.Revogar();
            await _unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
