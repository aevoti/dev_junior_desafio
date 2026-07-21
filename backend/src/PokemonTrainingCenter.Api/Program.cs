using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Api.Middleware;
using PokemonTrainingCenter.Domain.Repositories;
using PokemonTrainingCenter.Domain.Services;
using PokemonTrainingCenter.Infrastructure.Persistence;
using PokemonTrainingCenter.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevServerCorsPolicy = "AngularDevServer";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.MigrationsAssembly("PokemonTrainingCenter.Infrastructure")));

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<ITrainerRepository, TrainerRepository>();
builder.Services.AddScoped<IPokemonRepository, PokemonRepository>();
builder.Services.AddScoped<ITrainingPlanRepository, TrainingPlanRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

builder.Services.AddScoped<EnrollmentService>();

// Cria a política de CORS, que permite requisições vindas de http://localhost:4200
// aceitando qualquer header e qualquer método
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevServerCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors(AngularDevServerCorsPolicy);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
