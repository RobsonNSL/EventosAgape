using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Infraestrutura.Banco;
using AgapeApp.Infraestrutura.Banco.Repositorios;
using AgapeApp.Infraestrutura.Configuracoes;
using AgapeApp.Infraestrutura.Seguranca;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgapeApp.Infraestrutura;

/// <summary>
/// Registro dos serviços da camada de Infraestrutura no contêiner de injeção de dependência.
/// </summary>
public static class InjecaoDependenciasInfraestrutura
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var cadeiaConexao = configuracao.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "A cadeia de conexão 'ConnectionStrings:Postgres' não foi configurada.");

        servicos.AddDbContext<ContextoAgape>(opcoes =>
            opcoes.UseNpgsql(cadeiaConexao));

        servicos.Configure<OpcoesAutenticacao>(
            configuracao.GetSection(OpcoesAutenticacao.SecaoConfiguracao));

        servicos.AddScoped<IServicoHashSenha, ServicoHashSenhaArgon2>();
        servicos.AddScoped<IServicoToken, ServicoTokenJwt>();
        servicos.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
        servicos.AddScoped<IRepositorioTokensRenovacao, RepositorioTokensRenovacao>();
        servicos.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalho>();

        servicos.AddFluentMigratorCore()
            .ConfigureRunner(executor => executor
                .AddPostgres()
                .WithGlobalConnectionString(cadeiaConexao)
                .ScanIn(typeof(InjecaoDependenciasInfraestrutura).Assembly).For.Migrations())
            .AddLogging(opcoesDeLog => opcoesDeLog.AddFluentMigratorConsole());

        return servicos;
    }
}
