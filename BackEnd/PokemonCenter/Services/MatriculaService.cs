using Microsoft.EntityFrameworkCore;
using PokemonCenter.DTOs;
using PokemonCenter.Exceptions;
using PokemonCenter.Models;

namespace PokemonCenter.Services;

public class MatriculaService
{
    private readonly PokemonCenterContext _context;

    public MatriculaService(PokemonCenterContext context)
    {
        _context = context;
    }

    public async Task<MatriculaResponse> Criar(CriarMatriculaRequest novaMatricula)
    {
        var pokemon = await _context.Pokemons
            .Include(p => p.Treinador)
            .FirstOrDefaultAsync(p => p.Id == novaMatricula.PokemonId);
        var plano = await _context.Planos.FindAsync(novaMatricula.PlanoId);

        if (pokemon is null || plano is null)
        {
            throw new RecursoNaoEncontradoException("Pokémon e/ou plano não existente(s)");
        }

        if (plano.Nome == "Elite dos 4" && pokemon.Nivel < 50)
        {
            throw new RegraDeNegocioException("Pokémon não tem nível adequado para esse plano");
        }

        if (await _context.Matriculas.AnyAsync(m => m.PokemonId == pokemon.Id && m.Status == StatusMatricula.Ativa))
        {
            throw new RegraDeNegocioException("Esse Pokémon já possui uma matrícula ativa");
        }

        var matricula = new Matricula
        {
            PokemonId = pokemon.Id,
            PlanoId = plano.Id,
            Status = StatusMatricula.Ativa,
            DataInicio = DateOnly.FromDateTime(DateTime.Now),
            ValorPrimeiraCobranca = plano.ValorMensal
        };

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        return new MatriculaResponse(
            matricula.Id,
            matricula.Status,
            matricula.DataInicio,
            matricula.DataFim,
            matricula.ValorPrimeiraCobranca,
            pokemon.Id,
            pokemon.Nome,
            pokemon.Nivel,
            pokemon.TreinadorId,
            pokemon.Treinador.Nome,
            plano.Id,
            plano.Nome,
            plano.ValorMensal);
    }

    public async Task<List<MatriculaResponse>> ListarTodos()
    {
        return await _context.Matriculas
            .Select(m => new MatriculaResponse(
                m.Id,
                m.Status,
                m.DataInicio,
                m.DataFim,
                m.ValorPrimeiraCobranca,
                m.PokemonId,
                m.Pokemon.Nome,
                m.Pokemon.Nivel,
                m.Pokemon.TreinadorId,
                m.Pokemon.Treinador.Nome,
                m.PlanoId,
                m.Plano.Nome,
                m.Plano.ValorMensal))
            .ToListAsync();
    }

    private async Task<(Matricula atual, Plano novoPlano, decimal primeiraCobranca)> CalcularUpgrade(int matriculaId, int novoPlanoId)
    {
        var matricula = await _context.Matriculas
            .Include(m => m.Plano)
            .Include(m => m.Pokemon)
                .ThenInclude(p => p.Treinador)
            .FirstOrDefaultAsync(m => m.Id == matriculaId);
        var novoPlano = await _context.Planos.FindAsync(novoPlanoId);

        if (matricula is null || novoPlano is null)
        {
            throw new RecursoNaoEncontradoException("Matrícula ou plano não existem no sistema");
        }

        if (matricula.Status != StatusMatricula.Ativa)
        {
            throw new RegraDeNegocioException("Matrícula não está mais ativa");
        }

        if (novoPlano.ValorMensal <= matricula.Plano.ValorMensal)
        {
            throw new RegraDeNegocioException("Downgrade (plano inferior) não é permitido");
        }

        if (novoPlano.Nome == "Elite dos 4" && matricula.Pokemon.Nivel < 50)
        {
            throw new RegraDeNegocioException("Pokémon não tem nível adequado para esse plano");
        }

        var hoje = DateOnly.FromDateTime(DateTime.Now);

        const int diasCiclo = 30;
        var diasDecorridos = hoje.DayNumber - matricula.DataInicio.DayNumber;
        var diasDecorridosNoCiclo = diasDecorridos % diasCiclo;
        var diasRestantes = diasCiclo - diasDecorridosNoCiclo;

        decimal creditoDoPlanoAntigo = matricula.Plano.ValorMensal * (diasRestantes / (decimal)diasCiclo);
        decimal custonovoPlanoDiasRestantes = novoPlano.ValorMensal * (diasRestantes / (decimal)diasCiclo);
        decimal primeiraCobranca = custonovoPlanoDiasRestantes - creditoDoPlanoAntigo;

        return (matricula, novoPlano, primeiraCobranca);

    }

    public async Task<SimulacaoUpgradeResponse> SimularUpgrade(int matriculaId, int novoPlanoId)
    {
        var (_, _, primeiraCobranca) = await CalcularUpgrade(matriculaId, novoPlanoId);
        return new SimulacaoUpgradeResponse(primeiraCobranca);
    }

    public async Task<MatriculaResponse> Upgrade(int matriculaId, int novoPlanoId)
    {
        var (matricula, novoPlano, primeiraCobranca) = await CalcularUpgrade(matriculaId, novoPlanoId);
        var hoje = DateOnly.FromDateTime(DateTime.Now);

        matricula.Status = StatusMatricula.Concluida;
        matricula.DataFim = hoje;

        var novaMatricula = new Matricula
        {
            PokemonId = matricula.PokemonId,
            PlanoId = novoPlano.Id,
            DataInicio = hoje,
            Status = StatusMatricula.Ativa,
            ValorPrimeiraCobranca = primeiraCobranca,
        };

        _context.Matriculas.Add(novaMatricula);
        await _context.SaveChangesAsync();

        return new MatriculaResponse(
            novaMatricula.Id,
            novaMatricula.Status,
            novaMatricula.DataInicio,
            novaMatricula.DataFim,
            novaMatricula.ValorPrimeiraCobranca,
            matricula.PokemonId,
            matricula.Pokemon.Nome,
            matricula.Pokemon.Nivel,
            matricula.Pokemon.TreinadorId,
            matricula.Pokemon.Treinador.Nome,
            novoPlano.Id,
            novoPlano.Nome,
            novoPlano.ValorMensal);
    }


    public async Task<MatriculaResponse> ObterPorId(int id)
    {
        var matricula = await _context.Matriculas
            .Include(m => m.Pokemon)
                .ThenInclude(p => p.Treinador)
            .Include(m => m.Plano)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (matricula is null)
        {
            throw new RecursoNaoEncontradoException("Matrícula não existente");
        }

        return new MatriculaResponse(
            matricula.Id,
            matricula.Status,
            matricula.DataInicio,
            matricula.DataFim,
            matricula.ValorPrimeiraCobranca,
            matricula.PokemonId,
            matricula.Pokemon.Nome,
            matricula.Pokemon.Nivel,
            matricula.Pokemon.TreinadorId,
            matricula.Pokemon.Treinador.Nome,
            matricula.PlanoId,
            matricula.Plano.Nome,
            matricula.Plano.ValorMensal);
    }

    public async Task<MatriculaResponse> Cancelar(int matriculaId)
    {
        var matricula = await _context.Matriculas
            .Include(m => m.Plano)
            .Include(m => m.Pokemon)
                .ThenInclude(p => p.Treinador)
            .FirstOrDefaultAsync(m => m.Id == matriculaId);

        if (matricula is null)
        {
            throw new RecursoNaoEncontradoException("Matrícula não existente");
        }

        if (matricula.Status != StatusMatricula.Ativa)
        {
            throw new RegraDeNegocioException("Matrícula não está ativa");
        }

        matricula.Status = StatusMatricula.Cancelada;
        matricula.DataFim = DateOnly.FromDateTime(DateTime.Now);

        await _context.SaveChangesAsync();

        return new MatriculaResponse(
            matricula.Id,
            matricula.Status,
            matricula.DataInicio,
            matricula.DataFim,
            matricula.ValorPrimeiraCobranca,
            matricula.PokemonId,
            matricula.Pokemon.Nome,
            matricula.Pokemon.Nivel,
            matricula.Pokemon.TreinadorId,
            matricula.Pokemon.Treinador.Nome,
            matricula.PlanoId,
            matricula.Plano.Nome,
            matricula.Plano.ValorMensal);
    }
}
