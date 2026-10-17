using AgapeApp.Application.Cartoes.DTOs;
using MediatR;

namespace AgapeApp.Application.Cartoes.Queries.ListarCartoes;

/// <summary>
/// Lista os cartões cadastrados, com filtro opcional por trecho do código.
/// Por padrão exibe apenas os ativos. Somente administradores.
/// </summary>
public record ListarCartoesQuery(
    string? Codigo = null,
    bool IncluirInativos = false) : IRequest<IReadOnlyList<CartaoDTO>>;
