using CentroTreinamentoPokemon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CentroTreinamentoPokemon.Infrastructure.Context;

public class CentroTreinamentoPokemonContext : DbContext
{
    public CentroTreinamentoPokemonContext(
        DbContextOptions<CentroTreinamentoPokemonContext> options)
        : base(options)
    {
    }

    public DbSet<Treinador> Treinadores => Set<Treinador>();

    public DbSet<Pokemon> Pokemons => Set<Pokemon>();

    public DbSet<PlanoTreinamento> PlanosTreinamento => Set<PlanoTreinamento>();

    public DbSet<Matricula> Matriculas => Set<Matricula>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CentroTreinamentoPokemonContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}