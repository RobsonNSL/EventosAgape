using AgapeApp.Application.Authentication;
using AgapeApp.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AgapeApp.Application;

/// <summary>
/// Registro dos serviços da camada de Aplicação no contêiner de injeção de dependência.
/// </summary>
public static class ApplicationDependencyInjection
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationDependencyInjection).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddScoped<SessionService>();

        return services;
    }
}
