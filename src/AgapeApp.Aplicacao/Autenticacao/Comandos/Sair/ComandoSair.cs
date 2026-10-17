using MediatR;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.Sair;

/// <summary>
/// Encerra a sessão revogando o token de renovação informado.
/// </summary>
public record ComandoSair(string TokenRenovacao) : IRequest<Unit>;
