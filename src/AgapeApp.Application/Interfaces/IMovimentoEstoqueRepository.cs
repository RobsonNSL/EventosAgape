using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Interfaces;

/// <summary>
/// Acesso às movimentações de estoque.
/// </summary>
public interface IMovimentoEstoqueRepository
{
    void Adicionar(MovimentoEstoque movimento);
}
