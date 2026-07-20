using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Treinador> Treinadores => Set<Treinador>();
    public DbSet<Pokemon> Pokemons => Set<Pokemon>();
    public DbSet<PlanoTreinamento> PlanosTreinamento => Set<PlanoTreinamento>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Treinador>(entity =>
        {
            entity.ToTable("Treinadores");
            entity.HasIndex(t => t.Email).IsUnique();
            entity.Property(t => t.Nome).HasMaxLength(150).IsRequired();
            entity.Property(t => t.Email).HasMaxLength(200).IsRequired();
            entity.Property(t => t.CidadeOrigem).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Pokemon>(entity =>
        {
            entity.ToTable("Pokemons");
            entity.Property(p => p.Nome).HasMaxLength(150).IsRequired();
            entity.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.HasOne(p => p.Treinador)
                  .WithMany(t => t.Pokemons)
                  .HasForeignKey(p => p.TreinadorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlanoTreinamento>(entity =>
        {
            entity.ToTable("PlanosTreinamento");
            entity.Property(p => p.Nome).HasMaxLength(100).IsRequired();
            entity.Property(p => p.ValorMensal).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.ToTable("Matriculas");
            entity.Property(m => m.ValorMensal).HasColumnType("decimal(10,2)");
            entity.Property(m => m.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasOne(m => m.Pokemon)
                  .WithMany(p => p.Matriculas)
                  .HasForeignKey(m => m.PokemonId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.PlanoTreinamento)
                  .WithMany(p => p.Matriculas)
                  .HasForeignKey(m => m.PlanoTreinamentoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
