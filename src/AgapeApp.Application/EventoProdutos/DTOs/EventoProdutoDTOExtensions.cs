using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.EventoProdutos.DTOs;

/// <summary>
/// Conversão de <see cref="EventoProduto"/> para <see cref="EventoProdutoDTO"/>.
/// </summary>
public static class EventoProdutoDTOExtensions
{
    public static EventoProdutoDTO ParaDTO(this EventoProduto eventoProduto, string produtoNome) => new(
        eventoProduto.Id,
        eventoProduto.EventoId,
        eventoProduto.ProdutoId,
        produtoNome,
        eventoProduto.Preco,
        eventoProduto.ControlaEstoque,
        eventoProduto.QuantidadeEstoque);
}
