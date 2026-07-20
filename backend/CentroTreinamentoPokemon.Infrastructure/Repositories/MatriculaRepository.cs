using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Domain.Enums;
using CentroTreinamentoPokemon.Domain.Repositories;
using CentroTreinamentoPokemon.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroTreinamentoPokemon.Infrastructure.Repositories;

public class MatriculaRepository : IMatriculaRepository
{
    private readonly CentroTreinamentoPokemonContext _context;

    public MatriculaRepository(
        CentroTreinamentoPokemonContext context)
    {
        _context = context;
    }

    public async Task<Matricula?> RecuperarPorIdAsync(int id)
    {
        Matricula? matricula = await _context.Matriculas
            .Include(matricula => matricula.Pokemon)
                .ThenInclude(pokemon => pokemon.Treinador)
            .Include(matricula => matricula.PlanoTreinamento)
            .FirstOrDefaultAsync(matricula => matricula.Id == id);

        return matricula;
    }

    public async Task<Matricula?> RecuperarAtivaPorPokemonIdAsync(
        int pokemonId)
    {
        Matricula? matricula = await _context.Matriculas
            .Include(matricula => matricula.Pokemon)
            .Include(matricula => matricula.PlanoTreinamento)
            .FirstOrDefaultAsync(
                matricula =>
                    matricula.PokemonId == pokemonId &&
                    matricula.Status ==
                        StatusMatriculaEnum.Ativa);

        return matricula;
    }

    public async Task<bool> ExisteMatriculaAtivaPorPokemonIdAsync(
        int pokemonId)
    {
        bool existe = await _context.Matriculas
            .AnyAsync(
                matricula =>
                    matricula.PokemonId == pokemonId &&
                    matricula.Status ==
                        StatusMatriculaEnum.Ativa);

        return existe;
    }

    public async Task<IList<Matricula>> ListarAsync(
        string? busca,
        StatusMatriculaEnum? status)
    {
        IQueryable<Matricula> consulta = _context.Matriculas
            .AsNoTracking()
            .Include(matricula => matricula.Pokemon)
                .ThenInclude(pokemon => pokemon.Treinador)
            .Include(matricula => matricula.PlanoTreinamento);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            string buscaNormalizada = busca.Trim();

            consulta = consulta.Where(
                matricula =>
                    matricula.Pokemon.Nome.Contains(
                        buscaNormalizada) ||
                    matricula.Pokemon.Treinador.Nome.Contains(
                        buscaNormalizada));
        }

        if (status.HasValue)
        {
            consulta = consulta.Where(
                matricula =>
                    matricula.Status == status.Value);
        }

        IList<Matricula> matriculas = await consulta
            .OrderByDescending(matricula => matricula.DataInicio)
            .ToListAsync();

        return matriculas;
    }

    public async Task InserirAsync(Matricula matricula)
    {
        await _context.Matriculas.AddAsync(matricula);
    }
}