using AgapeApp.Application.Interfaces;
using AgapeApp.Infrastructure.Database;
using AgapeApp.Infrastructure.Database.Repositories;
using AgapeApp.Infrastructure.Options;
using AgapeApp.Infrastructure.Relatorios;
using AgapeApp.Infrastructure.Security;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgapeApp.Infrastructure;

/// <summary>
/// Registro dos serviços da camada de Infraestrutura no contêiner de injeção de dependência.
/// </summary>
public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException(
                "A cadeia de conexão 'ConnectionStrings:SqlServer' não foi configurada.");

        services.AddDbContext<AgapeDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<AuthenticationOptions>(
            configuration.GetSection(AuthenticationOptions.SecaoConfiguracao));

        services.AddScoped<IPasswordHashService, Argon2PasswordHashService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITokenRenovacaoRepository, TokenRenovacaoRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<ICartaoRepository, CartaoRepository>();
        services.AddScoped<IEventoRepository, EventoRepository>();
        services.AddScoped<ISetorRepository, SetorRepository>();
        services.AddScoped<IEventoProdutoRepository, EventoProdutoRepository>();
        services.AddScoped<ISetorProdutoRepository, SetorProdutoRepository>();
        services.AddScoped<IEventoOperadorRepository, EventoOperadorRepository>();
        services.AddScoped<IMovimentoEstoqueRepository, MovimentoEstoqueRepository>();
        services.AddScoped<ILogAuditoriaRepository, LogAuditoriaRepository>();
        services.AddScoped<ISaldoCartaoEventoRepository, SaldoCartaoEventoRepository>();
        services.AddScoped<IRecargaRepository, RecargaRepository>();
        services.AddScoped<IVendaRepository, VendaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Exportação de relatórios em Excel (ClosedXML).
        services.AddScoped<IExportadorRelatorioEvento, ExportadorRelatorioEventoExcel>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddSqlServer()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(InfrastructureDependencyInjection).Assembly).For.Migrations())
            .AddLogging(logging => logging.AddFluentMigratorConsole());

        return services;
    }
}
