using AgapeApp.Aplicacao.Comum.Excecoes;
using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Aplicacao.Usuarios.Dtos;
using AgapeApp.Dominio.Entidades;
using MediatR;

namespace AgapeApp.Aplicacao.Usuarios.Comandos.CriarUsuario;

public class ManipuladorComandoCriarUsuario : IRequestHandler<ComandoCriarUsuario, UsuarioDetalhe>
{
    private readonly IRepositorioUsuarios _repositorioUsuarios;
    private readonly IServicoHashSenha _servicoHashSenha;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public ManipuladorComandoCriarUsuario(
        IRepositorioUsuarios repositorioUsuarios,
        IServicoHashSenha servicoHashSenha,
        IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _repositorioUsuarios = repositorioUsuarios;
        _servicoHashSenha = servicoHashSenha;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    public async Task<UsuarioDetalhe> Handle(ComandoCriarUsuario comando, CancellationToken cancellationToken)
    {
        var emailNormalizado = Usuario.NormalizarEmail(comando.Email);

        if (await _repositorioUsuarios.ExistePorEmailAsync(emailNormalizado, cancellationToken))
            throw new ExcecaoConflito("Já existe um usuário cadastrado com este e-mail.");

        var usuario = Usuario.Criar(
            comando.Nome,
            emailNormalizado,
            _servicoHashSenha.GerarHash(comando.Senha),
            comando.Perfil);

        _repositorioUsuarios.Adicionar(usuario);
        await _unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return new UsuarioDetalhe(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil.ToString(),
            usuario.Ativo,
            usuario.CriadoEm);
    }
}
