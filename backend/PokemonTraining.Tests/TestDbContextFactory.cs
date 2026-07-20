using Microsoft.EntityFrameworkCore;
using PokemonTraining.Api.Data;

namespace PokemonTraining.Tests;

internal static class TestDbContextFactory
{
    public static PokemonTrainingDbContext Criar()
    {
        var options = new DbContextOptionsBuilder<PokemonTrainingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PokemonTrainingDbContext(options);
    }
}
