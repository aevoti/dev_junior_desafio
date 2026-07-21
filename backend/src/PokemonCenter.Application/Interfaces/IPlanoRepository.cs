using PokemonCenter.Domain.Entities;

namespace PokemonCenter.Application.Interfaces;

public interface IPlanoRepository
{
    Task<Plano?> GetByIdAsync(int id);
    Task<List<Plano>> GetAllAsync();
}