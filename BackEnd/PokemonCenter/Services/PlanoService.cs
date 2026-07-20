using Microsoft.EntityFrameworkCore;
using PokemonCenter.DTOs;
using PokemonCenter.Models;

namespace PokemonCenter.Services;

public class PlanoService
{
    private readonly PokemonCenterContext _context;

    public PlanoService(PokemonCenterContext context)
    {
        _context = context;
    }

    public async Task<List<PlanoResponse>> ListarTodos()
    {
        return await _context.Planos
            .OrderBy(p => p.ValorMensal)
            .Select(p => new PlanoResponse(p.Id, p.Nome, p.Descricao, p.ValorMensal))
            .ToListAsync();
    }
}
