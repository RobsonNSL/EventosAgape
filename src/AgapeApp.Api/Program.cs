using System.Text;
using System.Text.Json.Serialization;
using AgapeApp.Api.Servicos;
using AgapeApp.Api.Tratamento;
using AgapeApp.Aplicacao;
using AgapeApp.Infraestrutura;
using AgapeApp.Infraestrutura.Configuracoes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var construtor = WebApplication.CreateBuilder(args);
var configuracao = construtor.Configuration;

// Serviços das camadas de Aplicação e Infraestrutura.
construtor.Services.AdicionarAplicacao();
construtor.Services.AdicionarInfraestrutura(configuracao);

// Controladores com enums serializados como texto (ex.: "Administrador").
construtor.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Autenticação JWT.
var secaoAutenticacao = configuracao.GetSection(OpcoesAutenticacao.SecaoConfiguracao);
var chaveSecreta = secaoAutenticacao["ChaveSecreta"]
    ?? throw new InvalidOperationException("A configuração 'Autenticacao:ChaveSecreta' é obrigatória.");

construtor.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        // Mantém os nomes originais das claims ("sub", "perfil", "nome"...).
        opcoes.MapInboundClaims = false;

        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = secaoAutenticacao["Emissor"],
            ValidateAudience = true,
            ValidAudience = secaoAutenticacao["Audiencia"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "nome",
            RoleClaimType = "perfil"
        };
    });

// Autorização: política exclusiva de administradores.
construtor.Services.AddAuthorization(opcoes =>
{
    opcoes.AddPolicy("SomenteAdministrador", politica =>
        politica.RequireRole("Administrador"));
});

// Tratamento global de exceções com ProblemDetails em português.
construtor.Services.AddExceptionHandler<TratadorExcecoesGlobais>();
construtor.Services.AddProblemDetails();

// Swagger (com autenticação Bearer).
construtor.Services.AddEndpointsApiExplorer();
construtor.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AgapeApp API",
        Version = "v1",
        Description = "API do AgapeApp — cartão pré-pago e gestão de eventos."
    });

    opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (sem o prefixo 'Bearer')."
    });

    opcoes.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
    });
});

// Na inicialização: aplica migrations pendentes e garante o primeiro administrador.
construtor.Services.AddHostedService<ServicoInicializacaoBanco>();

var aplicacao = construtor.Build();

aplicacao.UseExceptionHandler();

if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseSwagger();
    aplicacao.UseSwaggerUI(opcoes =>
        opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "AgapeApp API v1"));
}

// Em produção a API ficará atrás de um proxy reverso responsável pelo TLS.
aplicacao.UseAuthentication();
aplicacao.UseAuthorization();

aplicacao.MapControllers();

aplicacao.Run();
