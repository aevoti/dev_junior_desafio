using Microsoft.EntityFrameworkCore;
using PokemonTraining.Api.Data;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Entities;
using PokemonTraining.Api.Exceptions;

namespace PokemonTraining.Api.Services;

public class TreinadorService(PokemonTrainingDbContext context) : ITreinadorService
{
    public async Task<IReadOnlyList<TreinadorResponse>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Treinadores
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new TreinadorResponse(x.Id, x.Nome, x.Email, x.CidadeOrigem))
            .ToListAsync(cancellationToken);

    public async Task<TreinadorResponse> CriarAsync(
        CriarTreinadorRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var emailNormalizado = email.ToUpperInvariant();

        if (await context.Treinadores.AnyAsync(
                x => x.Email.ToUpper() == emailNormalizado,
                cancellationToken))
        {
            throw new ConflitoException("Já existe um treinador cadastrado com este e-mail.");
        }

        var treinador = new Treinador
        {
            Nome = request.Nome.Trim(),
            Email = email,
            CidadeOrigem = request.CidadeOrigem.Trim()
        };

        context.Treinadores.Add(treinador);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflitoException("Já existe um treinador cadastrado com este e-mail.");
        }

        return new TreinadorResponse(treinador.Id, treinador.Nome, treinador.Email, treinador.CidadeOrigem);
    }
}
