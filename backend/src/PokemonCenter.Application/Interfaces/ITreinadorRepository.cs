using PokemonCenter.Domain.Entities;

namespace PokemonCenter.Application.Interfaces;

public interface ITreinadorRepository
{
    Task<Treinador?> GetByIdAsync(int id);
    Task<Treinador?> GetByEmailAsync(string email);
    Task<List<Treinador>> GetAllAsync();
    Task<Treinador> AddAsync(Treinador treinador);
}