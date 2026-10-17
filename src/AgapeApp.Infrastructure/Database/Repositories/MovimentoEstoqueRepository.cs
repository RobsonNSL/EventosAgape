using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;

namespace AgapeApp.Infrastructure.Database.Repositories;

public class MovimentoEstoqueRepository : IMovimentoEstoqueRepository
{
    private readonly AgapeDbContext _context;

    public MovimentoEstoqueRepository(AgapeDbContext context)
    {
        _context = context;
    }

    public void Adicionar(MovimentoEstoque movimento) => _context.MovimentosEstoque.Add(movimento);
}
