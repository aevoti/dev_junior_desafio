using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Models;

namespace PokemonTrainingCenter.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Treinador> Treinadores { get; set; }
    public DbSet<Pokemon> Pokemons { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Treinador>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Email).IsUnique();
            e.Property(t => t.Nome).IsRequired().HasMaxLength(100);
            e.Property(t => t.Email).IsRequired().HasMaxLength(150);
            e.Property(t => t.CidadeOrigem).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Pokemon>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Nome).IsRequired().HasMaxLength(100);
            e.Property(p => p.Tipo).IsRequired().HasMaxLength(50);
            e.HasOne(p => p.Treinador)
             .WithMany(t => t.Pokemons)
             .HasForeignKey(p => p.TreinadorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Matricula>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.ValorMensal).HasColumnType("decimal(10,2)");
            e.Property(m => m.Plano).HasConversion<string>();
            e.Property(m => m.Status).HasConversion<string>();
            e.HasOne(m => m.Pokemon)
             .WithMany(p => p.Matriculas)
             .HasForeignKey(m => m.PokemonId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
