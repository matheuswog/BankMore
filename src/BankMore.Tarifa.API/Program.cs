using BankMore.Tarifa.Infrastructure.Data;
using BankMore.Tarifa.Infrastructure.Repositories;
using BankMore.Tarifa.Domain.Handlers;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using KafkaFlow;
using BankMore.Transferencia.Domain.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore Tarifa API", Version = "v1" });
    
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
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessarTarifaHandler).Assembly));

// Database
builder.Services.AddScoped<DatabaseContext>(provider => 
    new DatabaseContext(builder.Configuration.GetConnectionString("DefaultConnection") ?? ""));

builder.Services.AddScoped<BankMore.Tarifa.Domain.Interfaces.ITarifaRepository, TarifaRepository>();
builder.Services.AddScoped<BankMore.Tarifa.Domain.Interfaces.IIdempotenciaRepository, IdempotenciaRepository>();
builder.Services.AddScoped<BankMore.Tarifa.Domain.Interfaces.ITarifaEventProducer, BankMore.Tarifa.Infrastructure.Producers.TarifaEventProducer>();

builder.Services.AddKafka(kafka => kafka
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092" })
        .AddConsumer(consumer => consumer
            .Topic(builder.Configuration["Kafka:Topics:TransferenciasRealizadas"] ?? "transferencias-realizadas")
            .WithGroupId("tarifa-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<BankMore.Tarifa.API.Serializers.SystemTextJsonDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<BankMore.Tarifa.API.Handlers.TransferenciaRealizadaHandler>())
            )
        )
        .AddProducer<BankMore.Tarifa.Domain.Events.TarifaRealizadaEvent>(
            producer => producer
                .DefaultTopic(builder.Configuration["Kafka:Topics:TarifasRealizadas"] ?? "tarifas-realizadas")
                .AddMiddlewares(m => m.AddSerializer<BankMore.Tarifa.API.Serializers.SystemTextJsonSerializer>(sp => new BankMore.Tarifa.API.Serializers.SystemTextJsonSerializer()))
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
        CREATE TABLE IF NOT EXISTS tarifa (
            idtarifa TEXT(37) PRIMARY KEY,
            idcontacorrente TEXT(37) NOT NULL,
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
