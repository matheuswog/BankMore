using BankMore.Transferencia.Infrastructure.Data;
using BankMore.Transferencia.Infrastructure.Repositories;
using BankMore.Transferencia.Domain.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

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
builder.Services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();

// HttpClient
builder.Services.AddHttpClient();

// Kafka
builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .AddProducer("default", producer => producer
            .DefaultTopic("default-topic")
            .AddMiddlewares(middlewares => middlewares
                .AddSerializer<System.Text.Json.JsonSerializer>()
            )
        )
    )
);

// Services
builder.Services.AddScoped<BankMore.Transferencia.Infrastructure.Services.IKafkaService, BankMore.Transferencia.Infrastructure.Services.KafkaService>();

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

// Initialize database
await InitializeDatabase(app);

app.Run();

static async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    
    // Create tables
    var createTablesSql = @"
        CREATE TABLE IF NOT EXISTS Transferencia (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            IdentificacaoRequisicao TEXT NOT NULL UNIQUE,
            ContaOrigemId INTEGER NOT NULL,
            ContaDestinoId INTEGER NOT NULL,
            Valor REAL NOT NULL,
            DataTransferencia TEXT NOT NULL,
            Descricao TEXT,
            Processada INTEGER NOT NULL DEFAULT 0
        );

        CREATE INDEX IF NOT EXISTS IX_Transferencia_IdentificacaoRequisicao ON Transferencia(IdentificacaoRequisicao);
    ";

    await context.Connection.ExecuteAsync(createTablesSql);
}
