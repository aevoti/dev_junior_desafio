using PokemonCenter.Domain.Entities;

namespace PokemonCenter.Application.Interfaces;

public interface IPokemonRepository
{
    Task<Pokemon?> GetByIdAsync(int id); 
    Task<List<Pokemon>> GetAllAsync();
    Task<Pokemon> AddAsync(Pokemon pokemon);
    Task UpdateAsync(Pokemon pokemon); 
}