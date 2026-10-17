using AgapeApp.Application.Vendas.DTOs;
using MediatR;

namespace AgapeApp.Application.Vendas.Queries.ObterVenda;

/// <summary>
/// Obtém o detalhe de uma venda com os itens (repasse e conferência). Somente administradores.
/// </summary>
public record ObterVendaQuery(Guid Id) : IRequest<VendaDetalheDTO>;
