using Microsoft.EntityFrameworkCore;
using PokemonTraining.Api.Data;
using PokemonTraining.Api.DTOs;

namespace PokemonTraining.Api.Services;

public class PlanoTreinamentoService(PokemonTrainingDbContext context) : IPlanoTreinamentoService
{
    public async Task<IReadOnlyList<PlanoTreinamentoResponse>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.PlanosTreinamento
            .AsNoTracking()
            .OrderBy(x => x.Ordem)
            .Select(x => new PlanoTreinamentoResponse(
                x.Id,
                x.Nome,
                x.ValorMensal,
                x.Descricao,
                x.Ordem,
                x.NivelMinimo))
            .ToListAsync(cancellationToken);
}
