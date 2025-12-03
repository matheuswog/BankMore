using BankMore.ContaCorrente.Infrastructure.Data;
using BankMore.ContaCorrente.Infrastructure.Repositories;
using BankMore.ContaCorrente.Domain.Handlers;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using KafkaFlow;
using BankMore.Tarifa.Domain.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore ContaCorrente API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CadastrarContaCorrenteHandler).Assembly));

builder.Services.AddScoped<DatabaseContext>(provider => 
    new DatabaseContext(builder.Configuration.GetConnectionString("DefaultConnection") ?? ""));

        builder.Services.AddScoped<BankMore.ContaCorrente.Domain.Interfaces.IContaCorrenteRepository, ContaCorrenteRepository>();
        builder.Services.AddScoped<BankMore.ContaCorrente.Domain.Interfaces.IMovimentoRepository, MovimentoRepository>();
        builder.Services.AddScoped<BankMore.ContaCorrente.Domain.Interfaces.IIdempotenciaRepository, IdempotenciaRepository>();

builder.Services.AddHttpClient();

builder.Services.AddKafka(kafka => kafka
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092" })
        .AddConsumer(consumer => consumer
            .Topic(builder.Configuration["Kafka:Topics:TarifasRealizadas"] ?? "tarifas-realizadas")
            .WithGroupId("conta-corrente-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<BankMore.ContaCorrente.API.Serializers.SystemTextJsonDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<BankMore.ContaCorrente.API.Handlers.TarifaRealizadaHandler>())
            )
        )
    )
);

builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await InitializeDatabase(app);

app.Run();

static async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    
    var createTablesSql = @"
        CREATE TABLE IF NOT EXISTS contacorrente (
            idcontacorrente TEXT(37) PRIMARY KEY,
            numero INTEGER(10) NOT NULL UNIQUE,
            nome TEXT(100) NOT NULL,
            cpf TEXT(11),
            ativo INTEGER(1) NOT NULL DEFAULT 0,
            senha TEXT(100) NOT NULL,
            salt TEXT(100) NOT NULL,
            CHECK (ativo IN (0,1))
        );

        CREATE TABLE IF NOT EXISTS movimento (
            idmovimento TEXT(37) PRIMARY KEY,
            idcontacorrente TEXT(37) NOT NULL,
            datamovimento TEXT(25) NOT NULL,
            tipomovimento TEXT(1) NOT NULL,
            valor REAL NOT NULL,
            CHECK (tipomovimento IN ('C','D')),
            FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente)
        );

        CREATE TABLE IF NOT EXISTS idempotencia (
            chave_idempotencia TEXT(37) PRIMARY KEY,
            requisicao TEXT(1000),
            resultado TEXT(1000)
        );

        CREATE INDEX IF NOT EXISTS IX_movimento_idcontacorrente ON movimento(idcontacorrente);
    ";

    await context.Connection.ExecuteAsync(createTablesSql);
}
