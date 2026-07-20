using CentroTreinamentoPokemon.DataTransfer.Requests.Treinador;
using CentroTreinamentoPokemon.DataTransfer.Responses.Treinador;
using CentroTreinamentoPokemon.Domain.Exceptions;
using CentroTreinamentoPokemon.Domain.Repositories;
using CentroTreinamentoPokemon.Domain.Entities;

namespace CentroTreinamentoPokemon.Application.Services.Treinadores;

public class TreinadorService : ITreinadorService
{
    private readonly ITreinadorRepository treinadorRepository;
    private readonly IUnitOfWork unitOfWork;

    public TreinadorService(
        ITreinadorRepository treinadorRepository,
        IUnitOfWork unitOfWork)
    {
        this.treinadorRepository = treinadorRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<TreinadorResponse> CriarAsync(TreinadorRequest request)
    {
        bool existe = await treinadorRepository.ExistePorEmailAsync(request.Email);

        if (existe)
            throw new RegraNegocioException(
                "Já existe um treinador cadastrado com este e-mail.");

        Treinador treinador = new Treinador(
            request.Nome,
            request.Email,
            request.CidadeOrigem);

        await treinadorRepository.InserirAsync(treinador);
        await unitOfWork.CommitAsync();

        return MapearResponse(treinador);
    }

    public async Task<TreinadorResponse?> ObterPorIdAsync(int id)
    {
        Treinador? treinador =
            await treinadorRepository.RecuperarPorIdAsync(id);

        if (treinador is null)
            return null;

        return MapearResponse(treinador);
    }

    public async Task<IEnumerable<TreinadorResponse>> ListarAsync()
    {
        IEnumerable<Treinador> treinadores =
            await treinadorRepository.ListarAsync();

        return treinadores.Select(MapearResponse);
    }

    private static TreinadorResponse MapearResponse(
        Treinador treinador)
    {
        return new TreinadorResponse
        {
            Id = treinador.Id,
            Nome = treinador.Nome,
            Email = treinador.Email,
            CidadeOrigem = treinador.CidadeOrigem
        };
    }
}