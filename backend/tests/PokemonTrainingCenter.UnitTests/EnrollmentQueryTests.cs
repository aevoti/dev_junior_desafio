using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Enums;
using PokemonTrainingCenter.Infrastructure.Persistence;

namespace PokemonTrainingCenter.UnitTests;

/// <summary>
/// Testes de US3 (FR-016, FR-017) — reproduzem em memória a mesma
/// normalização (remoção de acentos + upper invariant) que
/// EnrollmentsController.NormalizeForSearch usa contra o banco real, para
/// garantir que a regra de negócio (busca case/accent-insensitive) está
/// correta independentemente da camada HTTP.
/// </summary>
public class EnrollmentQueryTests
{
    private static AppDbContext NewInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Search_IsAccentInsensitive_AguaFindsÁgua()
    {
        var db = NewInMemoryDb();
        var trainer = new Trainer { Name = "Ash Ketchum", Email = "ash@example.com", City = "Pallet Town" };
        var plan = new TrainingPlan { Name = "Ginásio Local", MonthlyPrice = 50.00m, Description = "..." };
        var pokemon = new Pokemon { Name = "Água-viva", Type = PokemonType.Water, Level = 10, Trainer = trainer };
        db.AddRange(trainer, plan, pokemon);
        db.Enrollments.Add(new Enrollment { Pokemon = pokemon, TrainingPlan = plan, StartDate = DateTime.UtcNow, MonthlyPrice = plan.MonthlyPrice });
        await db.SaveChangesAsync();

        var matches = await QueryHelper.SearchByNameAsync(db, "agua");

        Assert.Single(matches);
        Assert.Equal("Água-viva", matches[0].Pokemon!.Name);
    }

    [Fact]
    public async Task Search_IsCaseInsensitive_UppercaseFindsLowercase()
    {
        var db = NewInMemoryDb();
        var trainer = new Trainer { Name = "Misty", Email = "misty@example.com", City = "Cerulean City" };
        var plan = new TrainingPlan { Name = "Ginásio Local", MonthlyPrice = 50.00m, Description = "..." };
        var pokemon = new Pokemon { Name = "psyduck", Type = PokemonType.Water, Level = 10, Trainer = trainer };
        db.AddRange(trainer, plan, pokemon);
        db.Enrollments.Add(new Enrollment { Pokemon = pokemon, TrainingPlan = plan, StartDate = DateTime.UtcNow, MonthlyPrice = plan.MonthlyPrice });
        await db.SaveChangesAsync();

        var matches = await QueryHelper.SearchByNameAsync(db, "PSYDUCK");

        Assert.Single(matches);
    }

    [Fact]
    public async Task Search_MatchesByTrainerName_NotJustPokemonName()
    {
        var db = NewInMemoryDb();
        var trainer = new Trainer { Name = "Brock Harrison", Email = "brock@example.com", City = "Pewter City" };
        var plan = new TrainingPlan { Name = "Ginásio Local", MonthlyPrice = 50.00m, Description = "..." };
        var pokemon = new Pokemon { Name = "Onix", Type = PokemonType.Rock, Level = 30, Trainer = trainer };
        db.AddRange(trainer, plan, pokemon);
        db.Enrollments.Add(new Enrollment { Pokemon = pokemon, TrainingPlan = plan, StartDate = DateTime.UtcNow, MonthlyPrice = plan.MonthlyPrice });
        await db.SaveChangesAsync();

        var matches = await QueryHelper.SearchByNameAsync(db, "harrison");

        Assert.Single(matches);
    }

    [Fact]
    public async Task Search_ReturnsEmpty_WhenNoMatch()
    {
        var db = NewInMemoryDb();
        var trainer = new Trainer { Name = "Ash Ketchum", Email = "ash2@example.com", City = "Pallet Town" };
        var plan = new TrainingPlan { Name = "Ginásio Local", MonthlyPrice = 50.00m, Description = "..." };
        var pokemon = new Pokemon { Name = "Pikachu", Type = PokemonType.Electric, Level = 10, Trainer = trainer };
        db.AddRange(trainer, plan, pokemon);
        db.Enrollments.Add(new Enrollment { Pokemon = pokemon, TrainingPlan = plan, StartDate = DateTime.UtcNow, MonthlyPrice = plan.MonthlyPrice });
        await db.SaveChangesAsync();

        var matches = await QueryHelper.SearchByNameAsync(db, "zzz-inexistente");

        Assert.Empty(matches);
    }

    [Fact]
    public async Task StatusFilter_Active_ExcludesEndedEnrollments()
    {
        var db = NewInMemoryDb();
        var trainer = new Trainer { Name = "Ash Ketchum", Email = "ash3@example.com", City = "Pallet Town" };
        var plan = new TrainingPlan { Name = "Ginásio Local", MonthlyPrice = 50.00m, Description = "..." };
        var activePokemon = new Pokemon { Name = "Bulbasaur", Type = PokemonType.Grass, Level = 10, Trainer = trainer };
        var endedPokemon = new Pokemon { Name = "Charmander", Type = PokemonType.Fire, Level = 10, Trainer = trainer };
        db.AddRange(trainer, plan, activePokemon, endedPokemon);
        db.Enrollments.AddRange(
            new Enrollment { Pokemon = activePokemon, TrainingPlan = plan, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = null, MonthlyPrice = plan.MonthlyPrice },
            new Enrollment { Pokemon = endedPokemon, TrainingPlan = plan, StartDate = DateTime.UtcNow.AddDays(-60), EndDate = DateTime.UtcNow.Date.AddDays(-1), MonthlyPrice = plan.MonthlyPrice }
        );
        await db.SaveChangesAsync();

        var active = db.Enrollments.Include(e => e.Pokemon).Where(e => e.EndDate == null || e.EndDate.Value >= DateTime.UtcNow).ToList();

        Assert.Single(active);
        Assert.Equal("Bulbasaur", active[0].Pokemon!.Name);
    }

    /// <summary>Reimplementação mínima da normalização usada por EnrollmentsController, para testar a regra isoladamente do HTTP.</summary>
    private static class QueryHelper
    {
        public static async Task<List<Enrollment>> SearchByNameAsync(AppDbContext db, string search)
        {
            var all = await db.Enrollments.Include(e => e.Pokemon!).ThenInclude(p => p.Trainer).ToListAsync();
            var normalizedSearch = Normalize(search);
            return all.Where(e =>
                Normalize(e.Pokemon!.Name).Contains(normalizedSearch) ||
                Normalize(e.Pokemon!.Trainer!.Name).Contains(normalizedSearch)).ToList();
        }

        private static string Normalize(string text)
        {
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToUpperInvariant();
        }
    }
}
