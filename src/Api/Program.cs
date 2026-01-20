using Api.Filters;
using Aplicacao.Servicos;
using FluentValidation;
using Infra.Contexto;
using Infra.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationAsyncFilter>();
});

// registra o filtro no DI
builder.Services.AddScoped<FluentValidationAsyncFilter>();
builder.Services.AddValidatorsFromAssemblyContaining<Aplicacao.Validacoes.CadeiraCriacaoDtoValidador>();

builder.Services.AddScoped<CadeiraServico>();
builder.Services.AddScoped<AlocacaoServico>();

builder.Services.AddScoped<Aplicacao.Interfaces.IRepositorioCadeira, RepositorioCadeira>();
builder.Services.AddScoped<Aplicacao.Interfaces.IRepositorioAlocacao, RepositorioAlocacao>();

var connectionString = builder.Configuration.GetConnectionString("BancoDados")
    ?? "server=localhost;port=3306;database=alocacao_cadeiras;user=root;password=senha";

builder.Services.AddDbContext<AplicacaoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Customize model state invalid response to the standardized error format
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var traceId = context.HttpContext.TraceIdentifier;
        var errors = context.ModelState
            .Where(kvp => kvp.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value.Errors.Select(err => new { field = kvp.Key, message = err.ErrorMessage }))
            .ToList();

        var result = new
        {
            success = false,
            error = new { code = "VALIDATION_ERROR", message = "Erro de validação", details = (string?)null, traceId, action = "Corrija os campos e tente novamente." },
            validationErrors = errors
        };

        return new BadRequestObjectResult(result);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global error handling
app.UseMiddleware<Api.Middleware.ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
