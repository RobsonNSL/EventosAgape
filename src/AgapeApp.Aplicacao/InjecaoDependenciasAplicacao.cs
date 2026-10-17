using AgapeApp.Aplicacao.Autenticacao;
using AgapeApp.Aplicacao.Comum.Comportamentos;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AgapeApp.Aplicacao;

/// <summary>
/// Registro dos serviços da camada de Aplicação no contêiner de injeção de dependência.
/// </summary>
public static class InjecaoDependenciasAplicacao
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection servicos)
    {
        var assembly = typeof(InjecaoDependenciasAplicacao).Assembly;

        servicos.AddMediatR(configuracao =>
        {
            configuracao.RegisterServicesFromAssembly(assembly);
            configuracao.AddOpenBehavior(typeof(ComportamentoValidacao<,>));
        });

        servicos.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        servicos.AddScoped<EmissorDeSessao>();

        return servicos;
    }
}
