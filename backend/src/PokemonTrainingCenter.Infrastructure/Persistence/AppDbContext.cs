using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Repositories;

namespace PokemonTrainingCenter.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{

    // Injeta a string de conexão + configurações do banco (vem do Program.cs)
    // e chama o construtor da classe pai
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Trainers é uma property da classe com um getter embutido. A versão completa dessa linha seria:
    // 
    // public DbSet<Trainer> Trainers { 
    //  get { return Set<Trainer>(); }
    // }
    //
    // Set é um método de DbContext, e DbSet<T> é uma classe do EF Core que representa a coleção de
    // todas as linhas dessa entidade no banco
    //
    // Ou seja, Trainers é o ponto de entrada para consultar a entity Trainer no banco — não 
    // carrega os dados na hora, só quando algo como .ToList() é chamado
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Pokemon> Pokemons => Set<Pokemon>();
    public DbSet<TrainingPlan> TrainingPlans => Set<TrainingPlan>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Para todo campo do tipo DateTime em qualquer entidade do projeto, aplica
        // um conversor que corrige o problema de UTC.
        //
        // O SQL Server devolve esses campos sem a tag Kind=Utc. Estes conversores
        // recolocam a tag, e com isso o serializador JSON sabe que tem que colocar
        // o Z ao final da data, e o navegador compreende que se trata de uma hora UTC,
        // resolvendo o bug da exibição de datas na listagem de matrículas.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base(
            v => v,
            // ao ler do banco, recoloca a etiqueta UTC
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    private sealed class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter() : base(
            v => v,
            // ao ler do banco, recoloca a etiqueta UTC
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }

    /// <summary>
    /// Hook chamado pela classe base (DbContext) para configurar o mapeamento das entidades.
    /// </summary>
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

            // FR-027: snapshot do dono do Pokémon no momento da criação da
            // matrícula — sem coleção inversa em Trainer, já que nada no
            // domínio precisa navegar de Trainer para as matrículas que ele
            // já teve historicamente.
            entity.HasOne(e => e.Trainer)
                .WithMany()
                .HasForeignKey(e => e.TrainerId)
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

    /// <summary>
    /// Insere dados de seed na tabela de training plans via migration.
    /// </summary>
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
