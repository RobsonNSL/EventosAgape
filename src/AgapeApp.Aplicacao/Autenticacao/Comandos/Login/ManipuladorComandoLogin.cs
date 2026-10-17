using AgapeApp.Aplicacao.Autenticacao.Dtos;
using AgapeApp.Aplicacao.Comum.Excecoes;
using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Dominio.Entidades;
using MediatR;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.Login;

public class ManipuladorComandoLogin : IRequestHandler<ComandoLogin, RespostaLogin>
{
    private readonly IRepositorioUsuarios _repositorioUsuarios;
    private readonly IServicoHashSenha _servicoHashSenha;
    private readonly EmissorDeSessao _emissorDeSessao;

    public ManipuladorComandoLogin(
        IRepositorioUsuarios repositorioUsuarios,
        IServicoHashSenha servicoHashSenha,
        EmissorDeSessao emissorDeSessao)
    {
        _repositorioUsuarios = repositorioUsuarios;
        _servicoHashSenha = servicoHashSenha;
        _emissorDeSessao = emissorDeSessao;
    }

    public async Task<RespostaLogin> Handle(ComandoLogin comando, CancellationToken cancellationToken)
    {
        var emailNormalizado = Usuario.NormalizarEmail(comando.Email);

        var usuario = await _repositorioUsuarios.ObterPorEmailAsync(emailNormalizado, cancellationToken)
            ?? throw new ExcecaoAutenticacao("E-mail ou senha inválidos.");

        if (!usuario.Ativo)
            throw new ExcecaoAutenticacao("Este usuário está desativado. Procure um administrador.");

        if (!_servicoHashSenha.Verificar(comando.Senha, usuario.SenhaHash))
            throw new ExcecaoAutenticacao("E-mail ou senha inválidos.");

        return await _emissorDeSessao.EmitirAsync(usuario, comando.Dispositivo, cancellationToken);
    }
}
