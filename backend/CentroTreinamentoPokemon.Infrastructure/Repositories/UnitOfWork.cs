using CentroTreinamentoPokemon.Domain.Repositories;
using CentroTreinamentoPokemon.Infrastructure.Context;

namespace CentroTreinamentoPokemon.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CentroTreinamentoPokemonContext _context;

    public UnitOfWork(
        CentroTreinamentoPokemonContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync()
    {
        int quantidadeRegistros =
            await _context.SaveChangesAsync();

        return quantidadeRegistros;
    }
}