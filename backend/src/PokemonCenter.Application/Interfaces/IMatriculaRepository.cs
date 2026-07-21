using PokemonCenter.Domain.Entities;
using PokemonCenter.Domain.Enums;

namespace PokemonCenter.Application.Interfaces;

public interface IMatriculaRepository
{
    Task<Matricula?> GetByIdAsync(int id); 
    Task<List<Matricula>> GetAllAsync(string? busca, StatusMatricula? status);
    Task<Matricula?> GetMatriculaAtivaByPokemonIdAsync(int pokemonId); 
    Task<Matricula> AddAsync(Matricula matricula);
    Task UpdateAsync(Matricula matricula);
}