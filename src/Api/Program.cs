using Aplicacao.Servicos;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infra.Contexto;
using Infra.Repositorios;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
