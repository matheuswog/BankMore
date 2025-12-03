using BankMore.Transferencia.Infrastructure.Data;
using BankMore.Transferencia.Infrastructure.Repositories;
using BankMore.Transferencia.Domain.Handlers;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using KafkaFlow;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore Transferencia API", Version = "v1" });
    
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

// JWT Authentication
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

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EfetuarTransferenciaHandler).Assembly));

// Database
builder.Services.AddScoped<DatabaseContext>(provider => 
    new DatabaseContext(builder.Configuration.GetConnectionString("DefaultConnection") ?? ""));

// Repositories
builder.Services.AddScoped<BankMore.Transferencia.Domain.Interfaces.ITransferenciaRepository, TransferenciaRepository>();
builder.Services.AddScoped<BankMore.Transferencia.Domain.Interfaces.IIdempotenciaRepository, IdempotenciaRepository>();
builder.Services.AddScoped<BankMore.Transferencia.Domain.Interfaces.ITransferenciaEventProducer, BankMore.Transferencia.Infrastructure.Producers.TransferenciaEventProducer>();
builder.Services.AddScoped<BankMore.Transferencia.Domain.Interfaces.IHttpClientService, BankMore.Transferencia.Infrastructure.Services.HttpClientService>();

builder.Services.AddHttpClient("ContaCorrenteApi", client =>
{
    var baseUrl = builder.Configuration["ContaCorrenteApi:BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
});

builder.Services.AddKafka(kafka => kafka
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092" })
        .AddProducer<BankMore.Transferencia.Domain.Events.TransferenciaRealizadaEvent>(
            producer => producer
                .DefaultTopic(builder.Configuration["Kafka:Topics:TransferenciasRealizadas"] ?? "transferencias-realizadas")
                .AddMiddlewares(m => m.AddSerializer<BankMore.Transferencia.API.Serializers.SystemTextJsonSerializer>(sp => new BankMore.Transferencia.API.Serializers.SystemTextJsonSerializer()))
        )
    )
);

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
        CREATE TABLE IF NOT EXISTS transferencia (
            idtransferencia TEXT(37) PRIMARY KEY,
            idcontacorrente_origem TEXT(37) NOT NULL,
            idcontacorrente_destino TEXT(37) NOT NULL,
            datamovimento TEXT(25) NOT NULL,
            valor REAL NOT NULL
        );

        CREATE TABLE IF NOT EXISTS idempotencia (
            chave_idempotencia TEXT(37) PRIMARY KEY,
            requisicao TEXT(1000),
            resultado TEXT(1000)
        );
    ";

    await context.Connection.ExecuteAsync(createTablesSql);
}
