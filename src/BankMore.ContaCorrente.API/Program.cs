using BankMore.ContaCorrente.Infrastructure.Data;
using BankMore.ContaCorrente.Infrastructure.Repositories;
using BankMore.ContaCorrente.Domain.Handlers;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

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

builder.Services.AddHttpClient();

// TODO: Implementar Kafka posteriormente

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
        CREATE TABLE IF NOT EXISTS ContaCorrente (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Cpf TEXT NOT NULL UNIQUE,
            NomeTitular TEXT NOT NULL,
            NumeroConta TEXT NOT NULL UNIQUE,
            Senha TEXT NOT NULL,
            Ativo INTEGER NOT NULL DEFAULT 1,
            DataCriacao TEXT NOT NULL,
            DataInativacao TEXT
        );

        CREATE TABLE IF NOT EXISTS Movimento (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            IdentificacaoRequisicao TEXT NOT NULL,
            ContaCorrenteId INTEGER NOT NULL,
            TipoMovimento TEXT NOT NULL,
            Valor REAL NOT NULL,
            DataMovimento TEXT NOT NULL,
            Descricao TEXT,
            FOREIGN KEY (ContaCorrenteId) REFERENCES ContaCorrente(Id)
        );

        CREATE INDEX IF NOT EXISTS IX_Movimento_ContaCorrenteId ON Movimento(ContaCorrenteId);
        CREATE INDEX IF NOT EXISTS IX_Movimento_IdentificacaoRequisicao ON Movimento(IdentificacaoRequisicao);
    ";

    await context.Connection.ExecuteAsync(createTablesSql);
}
