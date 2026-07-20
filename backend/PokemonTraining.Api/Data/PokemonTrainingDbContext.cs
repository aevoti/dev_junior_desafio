using Microsoft.EntityFrameworkCore;
using PokemonTraining.Api.Entities;

namespace PokemonTraining.Api.Data;

public class PokemonTrainingDbContext(DbContextOptions<PokemonTrainingDbContext> options)
    : DbContext(options)
{
    public DbSet<Treinador> Treinadores => Set<Treinador>();
    public DbSet<Pokemon> Pokemons => Set<Pokemon>();
    public DbSet<PlanoTreinamento> PlanosTreinamento => Set<PlanoTreinamento>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarTreinador(modelBuilder);
        ConfigurarPokemon(modelBuilder);
        ConfigurarPlanoTreinamento(modelBuilder);
        ConfigurarMatricula(modelBuilder);
    }

    private static void ConfigurarTreinador(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Treinador>();

        entity.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
        entity.Property(x => x.CidadeOrigem).HasMaxLength(150).IsRequired();
        entity.HasIndex(x => x.Email).IsUnique();
    }

    private static void ConfigurarPokemon(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Pokemon>();

        entity.Property(x => x.Nome).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Tipo).HasMaxLength(80).IsRequired();
        entity.ToTable(table => table.HasCheckConstraint(
            "CK_Pokemons_Nivel",
            "[Nivel] >= 1 AND [Nivel] <= 100"));

        entity.HasOne(x => x.Treinador)
            .WithMany(x => x.Pokemons)
            .HasForeignKey(x => x.TreinadorId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarPlanoTreinamento(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PlanoTreinamento>();

        entity.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Descricao).HasMaxLength(300).IsRequired();
        entity.Property(x => x.ValorMensal).HasPrecision(10, 2);
        entity.HasIndex(x => x.Nome).IsUnique();
        entity.HasIndex(x => x.Ordem).IsUnique();
        entity.ToTable(table =>
        {
            table.HasCheckConstraint("CK_PlanosTreinamento_ValorMensal", "[ValorMensal] > 0");
            table.HasCheckConstraint("CK_PlanosTreinamento_Ordem", "[Ordem] > 0");
            table.HasCheckConstraint(
                "CK_PlanosTreinamento_NivelMinimo",
                "[NivelMinimo] >= 1 AND [NivelMinimo] <= 100");
        });

        entity.HasData(
            new PlanoTreinamento
            {
                Id = 1,
                Nome = "Ginásio Local",
                ValorMensal = 50.00m,
                Descricao = "Treinos básicos",
                Ordem = 1,
                NivelMinimo = 1
            },
            new PlanoTreinamento
            {
                Id = 2,
                Nome = "Liga Regional",
                ValorMensal = 120.00m,
                Descricao = "Treinos intermediários e batalhas simuladas",
                Ordem = 2,
                NivelMinimo = 1
            },
            new PlanoTreinamento
            {
                Id = 3,
                Nome = "Elite dos 4",
                ValorMensal = 300.00m,
                Descricao = "Preparação completa para a Liga",
                Ordem = 3,
                NivelMinimo = 50
            });
    }

    private static void ConfigurarMatricula(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Matricula>();

        entity.Property(x => x.DataInicio).IsRequired();
        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();
        entity.Property(x => x.ValorMensal).HasPrecision(10, 2);
        entity.Property(x => x.MotivoEncerramento).HasMaxLength(500);
        entity.ToTable(table => table.HasCheckConstraint(
            "CK_Matriculas_ValorMensal",
            "[ValorMensal] > 0"));

        entity.HasIndex(x => x.PokemonId, "IX_Matriculas_PokemonId");
        entity.HasIndex(x => x.PlanoTreinamentoId);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.PokemonId, "UX_Matriculas_PokemonId_Ativa")
            .IsUnique()
            .HasFilter("[Status] = 'Ativa'");

        entity.HasOne(x => x.Pokemon)
            .WithMany(x => x.Matriculas)
            .HasForeignKey(x => x.PokemonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.PlanoTreinamento)
            .WithMany(x => x.Matriculas)
            .HasForeignKey(x => x.PlanoTreinamentoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
