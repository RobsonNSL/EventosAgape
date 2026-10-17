using AgapeApp.Aplicacao.Usuarios.Comandos.CriarUsuario;
using AgapeApp.Aplicacao.Usuarios.Consultas.ListarUsuarios;
using AgapeApp.Aplicacao.Usuarios.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controladores;

/// <summary>
/// Administração de usuários. Acesso exclusivo de administradores.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = "SomenteAdministrador")]
public class UsuariosControlador : ControllerBase
{
    private readonly ISender _mediador;

    public UsuariosControlador(ISender mediador)
    {
        _mediador = mediador;
    }

    /// <summary>Cria um novo usuário (administrador ou operador).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDetalhe), StatusCodes.Status201Created)]
    public async Task<ActionResult<UsuarioDetalhe>> Criar(
        [FromBody] ComandoCriarUsuario comando,
        CancellationToken cancellationToken)
    {
        var usuario = await _mediador.Send(comando, cancellationToken);
        return Created($"/api/usuarios/{usuario.Id}", usuario);
    }

    /// <summary>Lista todos os usuários cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDetalhe>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDetalhe>>> Listar(CancellationToken cancellationToken)
        => Ok(await _mediador.Send(new ConsultaListarUsuarios(), cancellationToken));
}
