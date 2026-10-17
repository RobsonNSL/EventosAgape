using AgapeApp.Application.EventoProdutos.DTOs;
using MediatR;

namespace AgapeApp.Application.EventoProdutos.Queries.ListarProdutosDoEvento;

/// <summary>
/// Lista os produtos vinculados ao evento, com preço e estoque. Somente administradores.
/// </summary>
public record ListarProdutosDoEventoQuery(Guid EventoId) : IRequest<IReadOnlyList<EventoProdutoDTO>>;
