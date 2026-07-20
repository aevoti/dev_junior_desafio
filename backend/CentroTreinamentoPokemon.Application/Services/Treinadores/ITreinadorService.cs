using CentroTreinamentoPokemon.DataTransfer.Requests.Treinador;
using CentroTreinamentoPokemon.DataTransfer.Responses.Treinador;

namespace CentroTreinamentoPokemon.Application.Services.Treinadores;

public interface ITreinadorService
{
    Task<TreinadorResponse> CriarAsync(TreinadorRequest request);

    Task<TreinadorResponse?> ObterPorIdAsync(int id);

    Task<IEnumerable<TreinadorResponse>> ListarAsync();
}