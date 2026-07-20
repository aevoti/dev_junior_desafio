using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Domain.Repositories;
using CentroTreinamentoPokemon.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroTreinamentoPokemon.Infrastructure.Repositories;

public class PlanoTreinamentoRepository :
    IPlanoTreinamentoRepository
{
    private readonly CentroTreinamentoPokemonContext _context;

    public PlanoTreinamentoRepository(
        CentroTreinamentoPokemonContext context)
    {
        _context = context;
    }

    public async Task<PlanoTreinamento?> RecuperarPorIdAsync(
        int id)
    {
        PlanoTreinamento? plano =
            await _context.PlanosTreinamento
                .FirstOrDefaultAsync(plano => plano.Id == id);

        return plano;
    }

    public async Task<IList<PlanoTreinamento>> ListarAsync()
    {
        IList<PlanoTreinamento> planos =
            await _context.PlanosTreinamento
                .AsNoTracking()
                .OrderBy(plano => plano.NivelPlano)
                .ToListAsync();

        return planos;
    }
}