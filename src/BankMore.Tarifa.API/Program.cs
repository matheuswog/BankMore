using BankMore.Tarifa.Infrastructure.Data;
using BankMore.Tarifa.Infrastructure.Repositories;
using BankMore.Tarifa.Domain.Handlers;
using Dapper;
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

// Repositories
builder.Services.AddScoped<BankMore.Tarifa.Domain.Interfaces.ITarifaRepository, TarifaRepository>();

// Kafka será implementado posteriormente

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
        CREATE TABLE IF NOT EXISTS Tarifa (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ContaCorrenteId INTEGER NOT NULL,
            ValorTarifado REAL NOT NULL,
            DataTarifacao TEXT NOT NULL,
            Descricao TEXT,
            IdentificacaoTransferencia TEXT NOT NULL UNIQUE
        );

        CREATE INDEX IF NOT EXISTS IX_Tarifa_ContaCorrenteId ON Tarifa(ContaCorrenteId);
        CREATE INDEX IF NOT EXISTS IX_Tarifa_IdentificacaoTransferencia ON Tarifa(IdentificacaoTransferencia);
    ";

    await context.Connection.ExecuteAsync(createTablesSql);
}
