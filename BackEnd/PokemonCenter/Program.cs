using Microsoft.EntityFrameworkCore;
using PokemonCenter.Endpoints;
using PokemonCenter.Exceptions;
using PokemonCenter.Models;
using PokemonCenter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PokemonCenterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PokemonCenter")));

builder.Services.AddScoped<TreinadorService>();
builder.Services.AddScoped<PokemonService>();
builder.Services.AddScoped<MatriculaService>();
builder.Services.AddScoped<PlanoService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
// Erros gerados pela validacao nao sao pegos no ExceptionHandler e estao em padrao de mensagem diferente
builder.Services.AddValidation();

const string PoliticaCorsFrontend = "PoliticaCorsFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCorsFrontend, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseExceptionHandler();

app.UseCors(PoliticaCorsFrontend);

app.MapTreinadorEndpoints();
app.MapPokemonEndpoints();
app.MapMatriculaEndpoints();
app.MapPlanoEndpoints();

app.Run();
