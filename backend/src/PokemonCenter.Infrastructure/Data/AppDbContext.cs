using Microsoft.EntityFrameworkCore;
using PokemonCenter.Domain.Entities;

namespace PokemonCenter.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Treinador> Treinadores { get; set; } = null!;
    public DbSet<Pokemon> Pokemons { get; set; } = null!;
    public DbSet<Plano> Planos { get; set; } = null!;
    public DbSet<Matricula> Matriculas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Treinador>()
        .HasIndex(t => t.Email)
        .IsUnique();

    modelBuilder.Entity<Matricula>()
        .Property(m => m.Status)
        .HasConversion<string>();

    modelBuilder.Entity<Matricula>()
        .Property(m => m.ValorMensal)
        .HasPrecision(10, 2);

    modelBuilder.Entity<Plano>()
        .Property(p => p.ValorMensal)
        .HasPrecision(10, 2);

    modelBuilder.Entity<Pokemon>()
        .Property(p => p.Tipo)
        .HasConversion<string>();

    modelBuilder.Entity<Plano>().HasData(
        new Plano { Id = 1, Nome = "Ginásio Local", ValorMensal = 50.00m, NivelMinimoRequerido = 1 },
        new Plano { Id = 2, Nome = "Liga Regional", ValorMensal = 120.00m, NivelMinimoRequerido = 1 },
        new Plano { Id = 3, Nome = "Elite dos 4", ValorMensal = 300.00m, NivelMinimoRequerido = 50 }
    );

    base.OnModelCreating(modelBuilder);
}
}