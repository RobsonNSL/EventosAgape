using AgapeApp.Aplicacao.Interfaces;

namespace AgapeApp.Infraestrutura.Banco;

public class UnidadeDeTrabalho : IUnidadeDeTrabalho
{
    private readonly ContextoAgape _contexto;

    public UnidadeDeTrabalho(ContextoAgape contexto)
    {
        _contexto = contexto;
    }

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _contexto.SaveChangesAsync(cancellationToken);
}
