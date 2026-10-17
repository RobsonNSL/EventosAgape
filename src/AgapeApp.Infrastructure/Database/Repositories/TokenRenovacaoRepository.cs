using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class TokenRenovacaoRepository : ITokenRenovacaoRepository
{
    private readonly AgapeDbContext _context;

    public TokenRenovacaoRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(TokenRenovacao token) => _context.TokensRenovacao.Add(token);

    // Sem AsNoTracking: a entidade é revogada em memória e persistida na unidade de trabalho.
    public Task<TokenRenovacao?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _context.TokensRenovacao
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
}
