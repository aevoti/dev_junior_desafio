using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Enums;
using PokemonTrainingCenter.Domain.Exceptions;
using PokemonTrainingCenter.Domain.Services;
using PokemonTrainingCenter.Infrastructure.Persistence;
using PokemonTrainingCenter.Infrastructure.Repositories;

namespace PokemonTrainingCenter.UnitTests;

public class EnrollmentServiceTests
{
    private static AppDbContext NewInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static EnrollmentService NewSut(AppDbContext db) => new(
        new EnrollmentRepository(db),
        new PokemonRepository(db),
        new TrainingPlanRepository(db),
        new TrainerRepository(db),
        db);

    private static async Task<(AppDbContext Db, EnrollmentService Sut, Trainer Trainer, TrainingPlan GinasioLocal, TrainingPlan EliteDosQuatro)> ArrangeAsync()
    {
        var db = NewInMemoryDb();

        var trainer = new Trainer { Name = "Ash Ketchum", Email = "ash@example.com", City = "Pallet Town" };
        var ginasioLocal = new TrainingPlan { Name = "Ginásio Local", MonthlyPrice = 50.00m, Description = "Treinos básicos" };
        var eliteDosQuatro = new TrainingPlan { Name = "Elite dos 4", MonthlyPrice = 300.00m, Description = "Preparação completa para a Liga" };

        db.Trainers.Add(trainer);
        db.TrainingPlans.AddRange(ginasioLocal, eliteDosQuatro);
        await db.SaveChangesAsync();

        return (db, NewSut(db), trainer, ginasioLocal, eliteDosQuatro);
    }

    // R1 — matrícula única ativa.

    [Fact]
    public async Task CreateEnrollmentAsync_RejectsSecondActiveEnrollment_ForSamePokemon()
    {
        var (db, sut, trainer, plan, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Pikachu", Type = PokemonType.Electric, Level = 25, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        await sut.CreateEnrollmentAsync(pokemon.Id, plan.Id);

        var ex = await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.CreateEnrollmentAsync(pokemon.Id, plan.Id));

        Assert.Equal("Este Pokémon já possui uma matrícula ativa.", ex.Message);
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task CreateEnrollmentAsync_RejectsNewEnrollment_WhenExistingEnrollmentEndsLaterToday()
    {
        // Edge case (spec.md Edge Cases): matrícula cancelada com término hoje
        // ainda conta como ativa até o fim do dia (FR-012/FR-020) — igual ao
        // que CancelEnrollmentAsync grava (fim do dia UTC, não o começo).
        var (db, sut, trainer, plan, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Pikachu", Type = PokemonType.Electric, Level = 25, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        db.Enrollments.Add(new Enrollment
        {
            PokemonId = pokemon.Id,
            TrainingPlanId = plan.Id,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1),
            MonthlyPrice = plan.MonthlyPrice
        });
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.CreateEnrollmentAsync(pokemon.Id, plan.Id));

        Assert.Equal("Este Pokémon já possui uma matrícula ativa.", ex.Message);
    }

    [Fact]
    public async Task CreateEnrollmentAsync_Allows_WhenExistingEnrollmentEndDateIsInThePast()
    {
        // FR-014 — matrícula encerrada não bloqueia nova matrícula.
        var (db, sut, trainer, plan, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Pikachu", Type = PokemonType.Electric, Level = 25, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        db.Enrollments.Add(new Enrollment
        {
            PokemonId = pokemon.Id,
            TrainingPlanId = plan.Id,
            StartDate = DateTime.UtcNow.AddDays(-60),
            EndDate = DateTime.UtcNow.Date.AddDays(-1),
            MonthlyPrice = plan.MonthlyPrice
        });
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, plan.Id);

        Assert.NotEqual(0, enrollment.Id);
        Assert.Null(enrollment.EndDate);
    }

    // R3 — nível mínimo para Elite dos 4.

    [Fact]
    public async Task CreateEnrollmentAsync_RejectsEliteDosQuatro_WhenLevelBelow50()
    {
        var (db, sut, trainer, _, eliteDosQuatro) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Charmander", Type = PokemonType.Fire, Level = 40, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.CreateEnrollmentAsync(pokemon.Id, eliteDosQuatro.Id));

        Assert.Equal("Nível mínimo de 50 é necessário para o plano Elite dos 4.", ex.Message);
    }

    [Fact]
    public async Task CreateEnrollmentAsync_AllowsEliteDosQuatro_WhenLevelIsExactly50()
    {
        var (db, sut, trainer, _, eliteDosQuatro) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Charizard", Type = PokemonType.Fire, Level = 50, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, eliteDosQuatro.Id);

        Assert.Equal(eliteDosQuatro.Id, enrollment.TrainingPlanId);
        Assert.Equal(300.00m, enrollment.MonthlyPrice);
    }

    [Fact]
    public async Task CreateEnrollmentAsync_AllowsOtherPlans_RegardlessOfLevel()
    {
        var (db, sut, trainer, ginasioLocal, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Caterpie", Type = PokemonType.Bug, Level = 5, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);

        Assert.Null(enrollment.EndDate);
        Assert.Equal(ginasioLocal.Id, enrollment.TrainingPlanId);
    }

    // R2 — upgrade: downgrade rejeitado, nível mínimo, e a transição do ConfirmUpgrade (FR-010).

    [Fact]
    public async Task PreviewUpgradeAsync_RejectsDowngrade_ToLowerOrEqualPricedPlan()
    {
        var (db, sut, trainer, ginasioLocal, _) = await ArrangeAsync();
        var ligaRegional = new TrainingPlan { Name = "Liga Regional", MonthlyPrice = 120.00m, Description = "..." };
        db.TrainingPlans.Add(ligaRegional);
        var pokemon = new Pokemon { Name = "Pikachu", Type = PokemonType.Electric, Level = 25, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, ligaRegional.Id);

        var ex = await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.PreviewUpgradeAsync(enrollment.Id, ginasioLocal.Id));

        Assert.Equal("Downgrade de plano não é permitido.", ex.Message);
    }

    [Fact]
    public async Task PreviewUpgradeAsync_RejectsSamePricedPlan_AsDowngrade()
    {
        var (db, sut, trainer, ginasioLocal, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Pikachu", Type = PokemonType.Electric, Level = 25, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);

        await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.PreviewUpgradeAsync(enrollment.Id, ginasioLocal.Id));
    }

    [Fact]
    public async Task PreviewUpgradeAsync_RejectsEliteDosQuatro_WhenLevelBelow50()
    {
        var (db, sut, trainer, ginasioLocal, eliteDosQuatro) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Charmander", Type = PokemonType.Fire, Level = 45, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);

        var ex = await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.PreviewUpgradeAsync(enrollment.Id, eliteDosQuatro.Id));

        Assert.Equal("Nível mínimo de 50 é necessário para o plano Elite dos 4.", ex.Message);
    }

    [Fact]
    public async Task PreviewUpgradeAsync_RejectsCancelledEnrollment_EvenWhileStillActive()
    {
        // FR-008: bug encontrado em teste manual (Session 2026-07-20 (3)) —
        // uma matrícula cancelada (FR-012) continua IsActive == true até o
        // fim do ciclo pago ("Ativa a encerrar"), mas não deve mais aceitar upgrade.
        var (db, sut, trainer, ginasioLocal, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Squirtle", Type = PokemonType.Water, Level = 10, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);
        var cancelled = await sut.CancelEnrollmentAsync(enrollment.Id);
        Assert.True(cancelled.IsActive); // ainda "Ativa a encerrar", não "Encerrada"

        var ex = await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.PreviewUpgradeAsync(enrollment.Id, ligaRegionalFrom(db).Id));

        Assert.Equal("Esta matrícula já foi cancelada e não pode receber upgrade.", ex.Message);
    }

    [Fact]
    public async Task PreviewUpgradeAsync_RejectsAlreadyEndedEnrollment()
    {
        // Verificado também via curl direto contra a API real (Session
        // 2026-07-20 (3)): já funcionava antes deste fix, mas sem teste dedicado.
        var (db, sut, trainer, ginasioLocal, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Squirtle", Type = PokemonType.Water, Level = 10, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);
        var tracked = await db.Enrollments.FirstAsync(e => e.Id == enrollment.Id);
        tracked.EndDate = DateTime.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.PreviewUpgradeAsync(enrollment.Id, ligaRegionalFrom(db).Id));

        Assert.Equal("Esta matrícula não está ativa.", ex.Message);
    }

    private static TrainingPlan ligaRegionalFrom(AppDbContext db)
    {
        var plan = new TrainingPlan { Name = "Liga Regional", MonthlyPrice = 120.00m, Description = "..." };
        db.TrainingPlans.Add(plan);
        db.SaveChanges();
        return plan;
    }

    [Fact]
    public async Task ConfirmUpgradeAsync_ClosesOldEnrollment_AndCreatesNewActiveOne()
    {
        var (db, sut, trainer, ginasioLocal, eliteDosQuatro) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Charizard", Type = PokemonType.Fire, Level = 55, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var original = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);

        var (closed, created, proration) = await sut.ConfirmUpgradeAsync(original.Id, eliteDosQuatro.Id);

        Assert.Equal(original.Id, closed.Id);
        Assert.NotNull(closed.EndDate);
        Assert.Equal(DateTime.UtcNow.Date, closed.EndDate!.Value.Date);
        // FR-020: upgrade encerra a matrícula anterior no instante exato, não só na virada do dia.
        Assert.False(closed.IsActive);

        Assert.NotEqual(closed.Id, created.Id);
        Assert.Null(created.EndDate);
        Assert.Equal(eliteDosQuatro.Id, created.TrainingPlanId);
        Assert.Equal(pokemon.Id, created.PokemonId);
        Assert.Equal(eliteDosQuatro.MonthlyPrice, created.MonthlyPrice);
        Assert.True(proration.FirstChargeAmount >= 0);

        // Nenhuma outra matrícula sem data de término (aberta) existe para o Pokémon além da nova.
        var openEnrollments = db.Enrollments.Count(e => e.PokemonId == pokemon.Id && e.EndDate == null);
        Assert.Equal(1, openEnrollments);
    }

    // R4 — cancelamento.

    [Fact]
    public async Task CancelEnrollmentAsync_SetsEndDateToCurrentCycleEnd_WithoutRefund()
    {
        var (db, sut, trainer, ginasioLocal, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Squirtle", Type = PokemonType.Water, Level = 10, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var enrollment = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);

        var cancelled = await sut.CancelEnrollmentAsync(enrollment.Id);

        Assert.NotNull(cancelled.EndDate);
        Assert.True(cancelled.EndDate!.Value >= DateTime.UtcNow.Date);
        // FR-020: EndDate futuro/hoje ainda conta como ativa ("a encerrar") até a virada do dia do ciclo.
        Assert.True(cancelled.IsActive);
    }

    [Fact]
    public async Task CancelEnrollmentAsync_RejectsAlreadyEndedEnrollment()
    {
        var (db, sut, trainer, plan, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Squirtle", Type = PokemonType.Water, Level = 10, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        db.Enrollments.Add(new Enrollment
        {
            PokemonId = pokemon.Id,
            TrainingPlanId = plan.Id,
            StartDate = DateTime.UtcNow.AddDays(-60),
            EndDate = DateTime.UtcNow.Date.AddDays(-1),
            MonthlyPrice = plan.MonthlyPrice
        });
        await db.SaveChangesAsync();
        var ended = db.Enrollments.Single();

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => sut.CancelEnrollmentAsync(ended.Id));
        Assert.Equal("Esta matrícula já está encerrada.", ex.Message);
    }

    // R5 — transferência com recriação automática de matrícula.

    [Fact]
    public async Task TransferPokemonAsync_ClosesOldEnrollment_AndCreatesNewOne_UnderDestinationTrainer()
    {
        var (db, sut, trainerA, ginasioLocal, _) = await ArrangeAsync();
        var trainerB = new Trainer { Name = "Misty", Email = "misty@example.com", City = "Cerulean City" };
        db.Trainers.Add(trainerB);
        var pokemon = new Pokemon { Name = "Psyduck", Type = PokemonType.Water, Level = 20, TrainerId = trainerA.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var original = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);

        var (transferredPokemon, closed, created) = await sut.TransferPokemonAsync(pokemon.Id, trainerB.Id);

        Assert.Equal(trainerB.Id, transferredPokemon.TrainerId);

        Assert.NotNull(closed);
        Assert.Equal(original.Id, closed!.Id);
        Assert.Equal(DateTime.UtcNow.Date, closed.EndDate!.Value.Date);
        // FR-020: transferência encerra a matrícula anterior no instante exato, não só na virada do dia.
        Assert.False(closed.IsActive);

        Assert.NotNull(created);
        Assert.Null(created!.EndDate);
        Assert.Equal(ginasioLocal.Id, created.TrainingPlanId);
        Assert.Equal(ginasioLocal.MonthlyPrice, created.MonthlyPrice); // cobrança integral, sem pro-rata

        // FR-027: a matrícula fechada preserva o Treinador de origem (histórico);
        // só a matrícula nova pertence ao Treinador de destino.
        Assert.Equal(trainerA.Id, closed.TrainerId);
        Assert.Equal(trainerB.Id, created.TrainerId);
    }

    [Fact]
    public async Task TransferPokemonAsync_DoesNotRewriteTrainerId_OfPastEnrollments()
    {
        // FR-027: transferir o Pokémon não deve reescrever o Treinador
        // histórico de nenhuma matrícula já existente, ativa ou encerrada.
        var (db, sut, trainerA, ginasioLocal, _) = await ArrangeAsync();
        var trainerB = new Trainer { Name = "Misty", Email = "misty3@example.com", City = "Cerulean City" };
        db.Trainers.Add(trainerB);
        var pokemon = new Pokemon { Name = "Psyduck", Type = PokemonType.Water, Level = 20, TrainerId = trainerA.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var original = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);
        await sut.TransferPokemonAsync(pokemon.Id, trainerB.Id);

        var reloaded = await db.Enrollments.AsNoTracking().FirstAsync(e => e.Id == original.Id);
        Assert.Equal(trainerA.Id, reloaded.TrainerId);
    }

    [Fact]
    public async Task TransferPokemonAsync_WithoutActiveEnrollment_OnlyChangesOwnership()
    {
        var (db, sut, trainerA, _, _) = await ArrangeAsync();
        var trainerB = new Trainer { Name = "Misty", Email = "misty@example.com", City = "Cerulean City" };
        db.Trainers.Add(trainerB);
        var pokemon = new Pokemon { Name = "Psyduck", Type = PokemonType.Water, Level = 20, TrainerId = trainerA.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var (transferredPokemon, closed, created) = await sut.TransferPokemonAsync(pokemon.Id, trainerB.Id);

        Assert.Equal(trainerB.Id, transferredPokemon.TrainerId);
        Assert.Null(closed);
        Assert.Null(created);
    }

    [Fact]
    public async Task TransferPokemonAsync_PicksTheCurrentEnrollment_EvenWhenAnOlderOneWasClosedTheSameDay()
    {
        // Regressão: encontrado via teste manual ponta a ponta contra o banco
        // real, num momento em que a checagem de "ativa" comparava só a
        // data (FR-020). Se um upgrade acontecia e, no mesmo dia, o Pokémon
        // era transferido, a matrícula ANTIGA (encerrada pelo upgrade) ainda
        // satisfazia essa checagem junto com a NOVA, e a transferência podia
        // pegar a matrícula errada (já superada), violando o índice único
        // ao tentar reabri-la. A comparação por instante exato (FR-020,
        // 2026-07-20) elimina a ambiguidade na origem: a matrícula antiga já
        // não satisfaz mais "ativa" assim que o upgrade é confirmado.
        var (db, sut, trainerA, ginasioLocal, _) = await ArrangeAsync();
        var trainerB = new Trainer { Name = "Misty", Email = "misty2@example.com", City = "Cerulean City" };
        db.Trainers.Add(trainerB);
        var ligaRegional = new TrainingPlan { Name = "Liga Regional", MonthlyPrice = 120.00m, Description = "..." };
        db.TrainingPlans.Add(ligaRegional);
        var pokemon = new Pokemon { Name = "Pikachu", Type = PokemonType.Electric, Level = 25, TrainerId = trainerA.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var original = await sut.CreateEnrollmentAsync(pokemon.Id, ginasioLocal.Id);
        var (_, upgraded, _) = await sut.ConfirmUpgradeAsync(original.Id, ligaRegional.Id);

        var (_, closed, created) = await sut.TransferPokemonAsync(pokemon.Id, trainerB.Id);

        Assert.NotNull(closed);
        Assert.Equal(upgraded.Id, closed!.Id); // a matrícula CORRETA (a nova, pós-upgrade), não a antiga já superada
        Assert.NotNull(created);
        Assert.Equal(ligaRegional.Id, created!.TrainingPlanId);

        // Nenhuma outra matrícula aberta (EndDate == null) restou para o Pokémon.
        var openCount = db.Enrollments.Count(e => e.PokemonId == pokemon.Id && e.EndDate == null);
        Assert.Equal(1, openCount);
    }

    [Fact]
    public async Task TransferPokemonAsync_RejectsTransferToSameTrainer()
    {
        var (db, sut, trainer, _, _) = await ArrangeAsync();
        var pokemon = new Pokemon { Name = "Psyduck", Type = PokemonType.Water, Level = 20, TrainerId = trainer.Id };
        db.Pokemons.Add(pokemon);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<DomainValidationException>(
            () => sut.TransferPokemonAsync(pokemon.Id, trainer.Id));

        Assert.Equal("O Pokémon já pertence a este Treinador.", ex.Message);
    }
}
