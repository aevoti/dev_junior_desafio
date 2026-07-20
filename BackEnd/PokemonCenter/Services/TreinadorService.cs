using Microsoft.EntityFrameworkCore;
using PokemonCenter.DTOs;
using PokemonCenter.Exceptions;
using PokemonCenter.Models;

namespace PokemonCenter.Services;

public class TreinadorService
{
    private readonly PokemonCenterContext _context;

    public TreinadorService(PokemonCenterContext context)
    {
        _context = context;
    }

    public async Task<TreinadorResponse> Criar(CriarTreinadorRequest novoTreinador)
    {
        if (await _context.Treinadors.AnyAsync(t => t.Email == novoTreinador.Email))
        {
            throw new RegraDeNegocioException("E-mail já cadastrado");
        }

        var treinador = new Treinador
        {
            Nome = novoTreinador.Nome,
            Email = novoTreinador.Email,
            CidadeDeOrigem = novoTreinador.CidadeDeOrigem
        };

        _context.Treinadors.Add(treinador);
        await _context.SaveChangesAsync();

        return new TreinadorResponse(treinador.Id, treinador.Nome, treinador.Email, treinador.CidadeDeOrigem);
    }

    public async Task<List<TreinadorResponse>> ListarTodos()
    {
        return await _context.Treinadors.Select(t => new TreinadorResponse(t.Id, t.Nome, t.Email, t.CidadeDeOrigem)).ToListAsync();
    }

    public async Task<TreinadorResponse> ObterPorId(int id)
    {
        var treinador = await _context.Treinadors.FindAsync(id);

        if (treinador is null)
        {
            throw new RecursoNaoEncontradoException("Treinador não existe");
        }

        return new TreinadorResponse(treinador.Id, treinador.Nome, treinador.Email, treinador.CidadeDeOrigem);
    }
}
