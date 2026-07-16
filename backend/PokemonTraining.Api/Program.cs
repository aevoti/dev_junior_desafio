using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using PokemonTraining.Api.Data;
using PokemonTraining.Api.Exceptions;
using PokemonTraining.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddDbContext<PokemonTrainingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITreinadorService, TreinadorService>();
builder.Services.AddScoped<IPokemonService, PokemonService>();
builder.Services.AddScoped<IPlanoTreinamentoService, PlanoTreinamentoService>();
builder.Services.AddScoped<IMatriculaService, MatriculaService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.MapControllers();

app.Run();
