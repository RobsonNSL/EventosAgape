using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Dominio.Entidades;
using AgapeApp.Dominio.Enumeracoes;
using FluentMigrator.Runner;

namespace AgapeApp.Api.Servicos;

/// <summary>
/// Na inicialização da API:
/// 1) aplica as migrations pendentes (FluentMigrator — somente Up);
/// 2) garante a existência do primeiro usuário administrador (seção "PrimeiroAdministrador").
/// </summary>
public class ServicoInicializacaoBanco : IHostedService
{
    private readonly IServiceScopeFactory _fabricaDeEscopo;
    private readonly IConfiguration _configuracao;
    private readonly ILogger<ServicoInicializacaoBanco> _registrador;

    public ServicoInicializacaoBanco(
        IServiceScopeFactory fabricaDeEscopo,
        IConfiguration configuracao,
        ILogger<ServicoInicializacaoBanco> registrador)
    {
        _fabricaDeEscopo = fabricaDeEscopo;
        _configuracao = configuracao;
        _registrador = registrador;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var escopo = _fabricaDeEscopo.CreateScope();
        var servicos = escopo.ServiceProvider;

        try
        {
            var executorDeMigracoes = servicos.GetRequiredService<IMigrationRunner>();
            executorDeMigracoes.MigrateUp();
        }
        catch (Exception excecao)
        {
            _registrador.LogCritical(
                excecao,
                "Não foi possível conectar ao banco de dados para aplicar as migrations. " +
                "Verifique a cadeia de conexão 'ConnectionStrings:Postgres' no appsettings.");
            throw;
        }

        await GarantirPrimeiroAdministradorAsync(servicos, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task GarantirPrimeiroAdministradorAsync(
        IServiceProvider servicos,
        CancellationToken cancellationToken)
    {
        var repositorioUsuarios = servicos.GetRequiredService<IRepositorioUsuarios>();

        if (await repositorioUsuarios.ExisteAlgumAdministradorAsync(cancellationToken))
            return;

        var secao = _configuracao.GetSection("PrimeiroAdministrador");
        var nome = secao["Nome"] ?? "Administrador";
        var email = secao["Email"];
        var senha = secao["Senha"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            _registrador.LogWarning(
                "Nenhum administrador cadastrado e a seção 'PrimeiroAdministrador' não está configurada. " +
                "Nenhum usuário inicial foi criado.");
            return;
        }

        var servicoHashSenha = servicos.GetRequiredService<IServicoHashSenha>();
        var unidadeDeTrabalho = servicos.GetRequiredService<IUnidadeDeTrabalho>();

        var administrador = Usuario.Criar(
            nome,
            email,
            servicoHashSenha.GerarHash(senha),
            PerfilUsuario.Administrador);

        repositorioUsuarios.Adicionar(administrador);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        _registrador.LogInformation(
            "Usuário administrador inicial criado: {Email}. Troque a senha assim que possível.",
            administrador.Email);
    }
}
