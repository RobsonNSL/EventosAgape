using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.EventoProdutos.DTOs;

/// <summary>
/// Conversão de <see cref="EventoProduto"/> para <see cref="EventoProdutoDTO"/>.
/// </summary>
public static class EventoProdutoDTOExtensions
{
    public static EventoProdutoDTO ParaDTO(this EventoProduto eventoProduto, Produto? produto) => new(
        eventoProduto.Id,
        eventoProduto.EventoId,
        eventoProduto.ProdutoId,
        produto?.Nome ?? string.Empty,
        eventoProduto.Preco,
        eventoProduto.ControlaEstoque,
        eventoProduto.QuantidadeEstoque,
        produto?.CaminhoImagem);
}
