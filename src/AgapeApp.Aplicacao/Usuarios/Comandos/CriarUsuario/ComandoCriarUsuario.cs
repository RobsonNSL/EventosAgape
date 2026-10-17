using AgapeApp.Aplicacao.Usuarios.Dtos;
using AgapeApp.Dominio.Enumeracoes;
using MediatR;

namespace AgapeApp.Aplicacao.Usuarios.Comandos.CriarUsuario;

/// <summary>
/// Cria um novo usuário (administrador ou operador). Somente administradores.
/// </summary>
public record ComandoCriarUsuario(
    string Nome,
    string Email,
    string Senha,
    PerfilUsuario Perfil) : IRequest<UsuarioDetalhe>;
