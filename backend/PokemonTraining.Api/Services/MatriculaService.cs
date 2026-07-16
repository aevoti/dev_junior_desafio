using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<SimulacaoUpgradeResponse> SimularUpgradeAsync(
        int matriculaId,
        UpgradeMatriculaRequest request,
        CancellationToken cancellationToken = default)
    {
        var dataUpgrade = DateTime.UtcNow;
        var dados = await PrepararUpgradeAsync(
            matriculaId,
            request.NovoPlanoTreinamentoId,
            dataUpgrade,
            cancellationToken);

        return new SimulacaoUpgradeResponse(
            dados.Matricula.Id,
            dados.Matricula.PlanoTreinamentoId,
            dados.Matricula.PlanoTreinamento.Nome,
            dados.NovoPlano.Id,
            dados.NovoPlano.Nome,
            dados.ProRata.DiasRestantes,
            dados.ProRata.CreditoPlanoAnterior,
            dados.ProRata.CustoProporcionalNovoPlano,
            dados.ProRata.PrimeiraCobranca,
            dataUpgrade);
    }

    public async Task<UpgradeMatriculaResponse> RealizarUpgradeAsync(
        int matriculaId,
        UpgradeMatriculaRequest request,
        CancellationToken cancellationToken = default)
    {
        var dataUpgrade = DateTime.UtcNow;
        IDbContextTransaction? transaction = null;

        try
        {
            if (context.Database.IsRelational())
            {
                transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            }

            var dados = await PrepararUpgradeAsync(
                matriculaId,
                request.NovoPlanoTreinamentoId,
                dataUpgrade,
                cancellationToken);

            var matriculaAnterior = dados.Matricula;
            matriculaAnterior.Status = StatusMatricula.Concluida;
            matriculaAnterior.DataFim = dataUpgrade;
            matriculaAnterior.MotivoEncerramento = $"Upgrade para o plano {dados.NovoPlano.Nome}.";

            var novaMatricula = new Matricula
            {
                PokemonId = matriculaAnterior.PokemonId,
                Pokemon = matriculaAnterior.Pokemon,
                PlanoTreinamentoId = dados.NovoPlano.Id,
                PlanoTreinamento = dados.NovoPlano,
                DataInicio = dataUpgrade,
                DataFim = null,
                Status = StatusMatricula.Ativa,
                ValorMensal = dados.NovoPlano.ValorMensal,
                MotivoEncerramento = null
            };

            context.Matriculas.Add(novaMatricula);
            await context.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return new UpgradeMatriculaResponse(
                matriculaAnterior.Id,
                novaMatricula.Id,
                matriculaAnterior.PokemonId,
                matriculaAnterior.Pokemon.Nome,
                matriculaAnterior.PlanoTreinamentoId,
                matriculaAnterior.PlanoTreinamento.Nome,
                dados.NovoPlano.Id,
                dados.NovoPlano.Nome,
                dados.ProRata.DiasRestantes,
                dados.ProRata.CreditoPlanoAnterior,
                dados.ProRata.CustoProporcionalNovoPlano,
                dados.ProRata.PrimeiraCobranca,
                dataUpgrade);
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    private async Task<DadosUpgrade> PrepararUpgradeAsync(
        int matriculaId,
        int novoPlanoId,
        DateTime dataUpgrade,
        CancellationToken cancellationToken)
    {
        var matricula = await context.Matriculas
            .Include(x => x.Pokemon)
            .Include(x => x.PlanoTreinamento)
            .SingleOrDefaultAsync(x => x.Id == matriculaId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Matrícula não encontrada.");

        var novoPlano = await context.PlanosTreinamento
            .SingleOrDefaultAsync(x => x.Id == novoPlanoId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Plano de treinamento não encontrado.");

        if (matricula.Status != StatusMatricula.Ativa)
        {
            throw new RegraNegocioException("Somente matrículas ativas podem receber upgrade.");
        }

        if (novoPlano.Ordem <= matricula.PlanoTreinamento.Ordem)
        {
            throw new RegraNegocioException("O novo plano deve ser superior ao plano atual.");
        }

        if (matricula.Pokemon.Nivel < novoPlano.NivelMinimo)
        {
            throw new RegraNegocioException(
                "O Pokémon não possui o nível mínimo exigido para este plano.");
        }

        var proRata = CalculadoraProRata.Calcular(
            matricula.DataInicio,
            dataUpgrade,
            matricula.ValorMensal,
            novoPlano.ValorMensal);

        return new DadosUpgrade(matricula, novoPlano, proRata);
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

    private sealed record DadosUpgrade(
        Matricula Matricula,
        PlanoTreinamento NovoPlano,
        ResultadoProRata ProRata);
}
