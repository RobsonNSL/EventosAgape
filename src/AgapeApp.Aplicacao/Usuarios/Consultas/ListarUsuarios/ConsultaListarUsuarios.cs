using AgapeApp.Aplicacao.Usuarios.Dtos;
using MediatR;

namespace AgapeApp.Aplicacao.Usuarios.Consultas.ListarUsuarios;

/// <summary>
/// Lista todos os usuários cadastrados. Somente administradores.
/// </summary>
public record ConsultaListarUsuarios : IRequest<IReadOnlyList<UsuarioDetalhe>>;
