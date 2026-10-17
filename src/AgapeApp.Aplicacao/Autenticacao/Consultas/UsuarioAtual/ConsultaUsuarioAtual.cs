using AgapeApp.Aplicacao.Autenticacao.Dtos;
using MediatR;

namespace AgapeApp.Aplicacao.Autenticacao.Consultas.UsuarioAtual;

/// <summary>
/// Consulta os dados do usuário autenticado.
/// </summary>
public record ConsultaUsuarioAtual(Guid UsuarioId) : IRequest<UsuarioAutenticado>;
