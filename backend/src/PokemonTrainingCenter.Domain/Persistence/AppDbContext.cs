using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;

namespace PokemonTrainingCenter.Domain.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Pokemon> Pokemons => Set<Pokemon>();
    public DbSet<TrainingPlan> TrainingPlans => Set<TrainingPlan>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Email).IsRequired().HasMaxLength(320);
            entity.Property(t => t.City).IsRequired().HasMaxLength(200);
            // SQL Server's default collation is already case-insensitive,
            // so this unique index also enforces case-insensitive uniqueness (FR-002).
            entity.HasIndex(t => t.Email).IsUnique();
        });

        modelBuilder.Entity<Pokemon>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Level).IsRequired();
            entity.Property(p => p.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(p => p.Trainer)
                .WithMany(t => t.Pokemons)
                .HasForeignKey(p => p.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t => t.HasCheckConstraint("CK_Pokemon_Level", "[Level] BETWEEN 1 AND 100"));

            var fixedTypes = string.Join(", ", Enum.GetNames<Domain.Enums.PokemonType>().Select(n => $"'{n}'"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Pokemon_Type", $"[Type] IN ({fixedTypes})"));
        });

        modelBuilder.Entity<TrainingPlan>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.MonthlyPrice).HasColumnType("decimal(10,2)");
            entity.Property(p => p.Description).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(10,2)");
            entity.Ignore(e => e.IsActive);

            entity.HasOne(e => e.Pokemon)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(e => e.PokemonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TrainingPlan)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(e => e.TrainingPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PokemonId);

            // Defense in depth for R1 under concurrent requests (research.md item 5):
            // at most one enrollment per Pokémon with no scheduled end date.
            entity.HasIndex(e => e.PokemonId)
                .IsUnique()
                .HasFilter("[EndDate] IS NULL")
                .HasDatabaseName("UX_Enrollment_PokemonId_Open");
        });

        SeedTrainingPlans(modelBuilder);
    }

    private static void SeedTrainingPlans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrainingPlan>().HasData(
            new TrainingPlan
            {
                Id = 1,
                Name = "Ginásio Local",
                MonthlyPrice = 50.00m,
                Description = "Treinos básicos"
            },
            new TrainingPlan
            {
                Id = 2,
                Name = "Liga Regional",
                MonthlyPrice = 120.00m,
                Description = "Treinos intermediários + batalhas simuladas"
            },
            new TrainingPlan
            {
                Id = 3,
                Name = "Elite dos 4",
                MonthlyPrice = 300.00m,
                Description = "Preparação completa para a Liga"
            }
        );
    }
}
