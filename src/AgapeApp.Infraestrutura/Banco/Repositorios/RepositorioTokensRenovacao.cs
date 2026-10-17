using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infraestrutura.Banco.Repositorios;

public class RepositorioTokensRenovacao : IRepositorioTokensRenovacao
{
    private readonly ContextoAgape _contexto;

    public RepositorioTokensRenovacao(ContextoAgape contexto)
    {
        _contexto = contexto;
    }

    public void Adicionar(TokenRenovacao token) => _contexto.TokensRenovacao.Add(token);

    // Sem AsNoTracking: a entidade é revogada em memória e persistida na unidade de trabalho.
    public Task<TokenRenovacao?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _contexto.TokensRenovacao
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
}
