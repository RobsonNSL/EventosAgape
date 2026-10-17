using System.Diagnostics;
using System.Text.Json.Serialization;
using AgapeApp.Api.Services;
using AgapeApp.Api.ExceptionHandling;
using AgapeApp.Application;
using AgapeApp.Domain.Resources;
using AgapeApp.Infrastructure;
using AgapeApp.Infrastructure.Options;
using AgapeApp.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Serviços das camadas de Aplicação e Infraestrutura.
builder.Services.AdicionarAplicacao();
builder.Services.AdicionarInfraestrutura(configuration);

// Controladores com enums serializados como texto (ex.: "Administrador").
builder.Services
    .AddControllers(options =>
    {
        // Campos ausentes passam pelo FluentValidation (mensagens em português do resources),
        // em vez do 400 automático do [ApiController] ("The Email field is required.").
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Falhas reais de binding (JSON malformado, tipo incorreto) devolvem ProblemDetails em português.
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory = ModelStateProblemDetails.Criar);

// Autenticação JWT.
var authenticationSection = configuration.GetSection(AuthenticationOptions.SecaoConfiguracao);
var chaveSecreta = authenticationSection["ChaveSecreta"]
    ?? throw new InvalidOperationException("A configuração 'Autenticacao:ChaveSecreta' é obrigatória.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Mantém os nomes originais das claims ("sub", "perfil", "nome"...).
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authenticationSection["Emissor"],
            ValidateAudience = true,
            ValidAudience = authenticationSection["Audiencia"],
            ValidateIssuerSigningKey = true,
            // A chave é derivada via SHA-256 (JwtKeyDerivation): qualquer tamanho de segredo funciona com o HS256.
            IssuerSigningKey = new SymmetricSecurityKey(JwtKeyDerivation.Derivar(chaveSecreta)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "nome",
            RoleClaimType = "perfil"
        };

        // Respostas de 401/403 do middleware em ProblemDetails/português
        // (o padrão do JwtBearer volta sem corpo, o que confunde no Swagger).
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;

                var detalhe = !contexto.Request.Headers.ContainsKey("Authorization")
                    ? MensagensErro.TokenNaoInformado
                    : contexto.AuthenticateFailure is SecurityTokenExpiredException
                        ? MensagensErro.TokenExpirado
                        : MensagensErro.TokenInvalido;

                var problema = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = MensagensErro.TituloNaoAutenticado,
                    Detail = detalhe,
                    Instance = contexto.Request.Path
                };

                problema.Extensions["traceId"] = contexto.HttpContext.TraceIdentifier;

                await contexto.Response.WriteAsJsonAsync(problema, contexto.HttpContext.RequestAborted);
            },
            OnForbidden = async contexto =>
            {
                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;

                var problema = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = MensagensErro.TituloAcessoNegado,
                    Detail = MensagensErro.SemPermissao,
                    Instance = contexto.Request.Path
                };

                problema.Extensions["traceId"] = contexto.HttpContext.TraceIdentifier;

                await contexto.Response.WriteAsJsonAsync(problema, contexto.HttpContext.RequestAborted);
            }
        };
    });

// Autorização: política exclusiva de administradores.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SomenteAdministrador", policy =>
        policy.RequireRole("Administrador"));
});

// Tratamento global de exceções com ProblemDetails em português.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger (com autenticação Bearer).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AgapeApp API",
        Version = "v1",
        Description = "API do AgapeApp — cartão pré-pago e gestão de eventos."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (sem o prefixo 'Bearer')."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Na inicialização: aplica migrations pendentes e garante o primeiro administrador.
builder.Services.AddHostedService<DatabaseInitializationService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AgapeApp API v1"));

    // A raiz leva direto para a tela do Swagger.
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

    // Abre o Swagger no navegador ao iniciar a API (desligue com Swagger:AbrirNavegador = false).
    if (configuration.GetValue("Swagger:AbrirNavegador", true))
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                var endereco = app.Services.GetRequiredService<IServer>()
                    .Features.Get<IServerAddressesFeature>()?.Addresses
                    .FirstOrDefault(enderecoBind => enderecoBind.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    ?? "http://localhost:5180";

                endereco = endereco.Replace("0.0.0.0", "localhost").Replace("[::]", "localhost").TrimEnd('/');

                app.Logger.LogInformation("Abrindo o Swagger no navegador: {Endereco}", endereco);
                Process.Start(new ProcessStartInfo(endereco) { UseShellExecute = true });
            }
            catch (Exception excecao)
            {
                // Abrir o navegador é apenas uma conveniência: se falhar, a API continua normalmente.
                app.Logger.LogWarning(excecao, "Não foi possível abrir o navegador automaticamente.");
            }
        });
    }
}

// Em produção a API ficará atrás de um proxy reverso responsável pelo TLS.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
