using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using FluentMigrator.Runner;

namespace AgapeApp.Api.Services;

/// <summary>
/// Na inicialização da API:
/// 1) aplica as migrations pendentes (FluentMigrator — somente Up);
/// 2) garante a existência do primeiro usuário administrador (seção "PrimeiroAdministrador").
/// </summary>
public class DatabaseInitializationService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DatabaseInitializationService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var migrationRunner = services.GetRequiredService<IMigrationRunner>();
            migrationRunner.MigrateUp();
        }
        catch (Exception excecao)
        {
            _logger.LogCritical(
                excecao,
                "Não foi possível conectar ao banco de dados para aplicar as migrations. " +
                "Verifique a cadeia de conexão 'ConnectionStrings:SqlServer' no appsettings.");
            throw;
        }

        await GarantirPrimeiroAdministradorAsync(services, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task GarantirPrimeiroAdministradorAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var usuarioRepository = services.GetRequiredService<IUsuarioRepository>();

        if (await usuarioRepository.ExisteAlgumAdministradorAsync(cancellationToken))
            return;

        var section = _configuration.GetSection("PrimeiroAdministrador");
        var nome = section["Nome"] ?? "Administrador";
        var email = section["Email"];
        var senha = section["Senha"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            _logger.LogWarning(
                "Nenhum administrador cadastrado e a seção 'PrimeiroAdministrador' não está configurada. " +
                "Nenhum usuário inicial foi criado.");
            return;
        }

        var passwordHashService = services.GetRequiredService<IPasswordHashService>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();

        var administrador = Usuario.Criar(
            nome,
            email,
            passwordHashService.GerarHash(senha),
            PerfilUsuario.Administrador);

        usuarioRepository.Adicionar(administrador);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation(
            "Usuário administrador inicial criado: {Email}. Troque a senha assim que possível.",
            administrador.Email);
    }
}
