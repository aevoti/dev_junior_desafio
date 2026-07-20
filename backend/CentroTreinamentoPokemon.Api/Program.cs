using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using CentroTreinamentoPokemon.Domain.Repositories;
using CentroTreinamentoPokemon.Infrastructure.Repositories;
using CentroTreinamentoPokemon.Application.Services.Treinadores;
using CentroTreinamentoPokemon.Application.Services.PlanosTreinamento;
using CentroTreinamentoPokemon.Application.Services.Pokemons;
using CentroTreinamentoPokemon.Application.Services.Matriculas;
using CentroTreinamentoPokemon.Api.Middlewares;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITreinadorRepository, TreinadorRepository>();
builder.Services.AddScoped<IPokemonRepository, PokemonRepository>();
builder.Services.AddScoped<IPlanoTreinamentoRepository, PlanoTreinamentoRepository>();
builder.Services.AddScoped<IMatriculaRepository, MatriculaRepository>();


builder.Services.AddScoped<ITreinadorService, TreinadorService>();
builder.Services.AddScoped<IPlanoTreinamentoService, PlanoTreinamentoService>();
builder.Services.AddScoped<IPokemonService, PokemonService>();
builder.Services.AddScoped<IMatriculaService, MatriculaService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();



builder.Services.AddDbContext<CentroTreinamentoPokemonContext>(
    options =>
    {
        string connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "A string de conexão DefaultConnection não foi configurada.");

        options.UseSqlServer(connectionString);
    });

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "Frontend",
            policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();



using (IServiceScope scope = app.Services.CreateScope())
{
    CentroTreinamentoPokemonContext context =
        scope.ServiceProvider
            .GetRequiredService<CentroTreinamentoPokemonContext>();

    context.Database.Migrate();

    if (!context.PlanosTreinamento.Any())
    {
        IList<PlanoTreinamento> planos = new List<PlanoTreinamento>
        {
            new PlanoTreinamento(
                "Ginásio Local",
                50.00m,
                "Treinos básicos",
                1),

            new PlanoTreinamento(
                "Liga Regional",
                120.00m,
                "Treinos intermediários e batalhas simuladas",
                2),

            new PlanoTreinamento(
                "Elite dos 4",
                300.00m,
                "Preparação completa para a Liga",
                3)
        };

        context.PlanosTreinamento.AddRange(planos);

        context.SaveChanges();
    }
}

app.Run();