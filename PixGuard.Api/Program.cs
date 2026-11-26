using PixGuard.Api.Endpoints;
using PixGuard.Api.Infra;
using PixGuard.Api.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Singleton: o "banco" e o relógio são compartilhados por todas as requisições
builder.Services.AddSingleton<BancoEmMemoria>();
builder.Services.AddSingleton<RelogioControlavel>();
builder.Services.AddSingleton<IRelogio>(sp => sp.GetRequiredService<RelogioControlavel>());
builder.Services.AddSingleton<ContaService>();
builder.Services.AddSingleton<PixService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();          // contrato da API em /openapi/v1.json
    app.MapearApoioTestes();   // /test/* só existe fora de produção
}

app.MapearApi();

app.Run();

// Necessário para testes de integração enxergarem a classe Program
public partial class Program { }
