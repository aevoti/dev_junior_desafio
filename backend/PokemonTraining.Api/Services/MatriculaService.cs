using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PokemonTraining.Api.Data;
using PokemonTraining.Api.DTOs;
using PokemonTraining.Api.Entities;
using PokemonTraining.Api.Enums;
using PokemonTraining.Api.Exceptions;

namespace PokemonTraining.Api.Services;

public class MatriculaService(PokemonTrainingDbContext context) : IMatriculaService
{
    public async Task<IReadOnlyList<MatriculaResponse>> ListarAsync(
        string? busca,
        StatusMatricula? status,
        CancellationToken cancellationToken = default)
    {
        var query = context.Matriculas.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var termo = busca?.Trim();
        if (!string.IsNullOrWhiteSpace(termo))
        {
            query = query.Where(x =>
                x.Pokemon.Nome.Contains(termo) ||
                x.Pokemon.Treinador.Nome.Contains(termo));
        }

        return await query
            .OrderByDescending(x => x.DataInicio)
            .Select(x => new MatriculaResponse(
                x.Id,
                x.PokemonId,
                x.Pokemon.Nome,
                x.Pokemon.TreinadorId,
                x.Pokemon.Treinador.Nome,
                x.PlanoTreinamentoId,
                x.PlanoTreinamento.Nome,
                x.DataInicio,
                x.DataFim,
                x.Status,
                x.ValorMensal,
                x.MotivoEncerramento))
            .ToListAsync(cancellationToken);
    }

    public async Task<MatriculaResponse> CriarAsync(
        CriarMatriculaRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DataInicio == DateTime.MinValue)
        {
            throw new RegraNegocioException("A data de início deve ser informada.");
        }

        var pokemon = await context.Pokemons
            .Include(x => x.Treinador)
            .SingleOrDefaultAsync(x => x.Id == request.PokemonId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Pokémon não encontrado.");

        var plano = await context.PlanosTreinamento
            .SingleOrDefaultAsync(x => x.Id == request.PlanoTreinamentoId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Plano de treinamento não encontrado.");

        if (await context.Matriculas.AnyAsync(
                x => x.PokemonId == request.PokemonId && x.Status == StatusMatricula.Ativa,
                cancellationToken))
        {
            throw new ConflitoException("Este Pokémon já possui uma matrícula ativa.");
        }

        if (pokemon.Nivel < plano.NivelMinimo)
        {
            throw new RegraNegocioException("O Pokémon não possui o nível mínimo exigido para este plano.");
        }

        var matricula = new Matricula
        {
            PokemonId = pokemon.Id,
            Pokemon = pokemon,
            PlanoTreinamentoId = plano.Id,
            PlanoTreinamento = plano,
            DataInicio = request.DataInicio,
            DataFim = null,
            Status = StatusMatricula.Ativa,
            ValorMensal = plano.ValorMensal,
            MotivoEncerramento = null
        };

        context.Matriculas.Add(matricula);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (EhViolacaoDeUnicidade(exception))
        {
            throw new ConflitoException("Este Pokémon já possui uma matrícula ativa.");
        }

        return Mapear(matricula);
    }

    public async Task<MatriculaResponse> CancelarAsync(
        int id,
        CancelarMatriculaRequest request,
        CancellationToken cancellationToken = default)
    {
        var matricula = await context.Matriculas
            .Include(x => x.Pokemon)
                .ThenInclude(x => x.Treinador)
            .Include(x => x.PlanoTreinamento)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Matrícula não encontrada.");

        if (matricula.Status != StatusMatricula.Ativa)
        {
            throw new RegraNegocioException("Somente matrículas ativas podem ser canceladas.");
        }

        matricula.Status = StatusMatricula.Cancelada;
        matricula.DataFim = DateTime.UtcNow;
        matricula.MotivoEncerramento = string.IsNullOrWhiteSpace(request.Motivo)
            ? null
            : request.Motivo.Trim();

        await context.SaveChangesAsync(cancellationToken);
        return Mapear(matricula);
    }

    private static MatriculaResponse Mapear(Matricula matricula) => new(
        matricula.Id,
        matricula.PokemonId,
        matricula.Pokemon.Nome,
        matricula.Pokemon.TreinadorId,
        matricula.Pokemon.Treinador.Nome,
        matricula.PlanoTreinamentoId,
        matricula.PlanoTreinamento.Nome,
        matricula.DataInicio,
        matricula.DataFim,
        matricula.Status,
        matricula.ValorMensal,
        matricula.MotivoEncerramento);

    private static bool EhViolacaoDeUnicidade(DbUpdateException exception)
    {
        var current = exception.InnerException;
        while (current is not null)
        {
            if (current is SqlException sqlException && sqlException.Number is 2601 or 2627)
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }
}
