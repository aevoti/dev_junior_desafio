using CentroTreinamentoPokemon.DataTransfer.Responses.PlanoTreinamento;

namespace CentroTreinamentoPokemon.Application.Services.PlanosTreinamento;

public interface IPlanoTreinamentoService
{
    Task<PlanoTreinamentoResponse?> ObterPorIdAsync(int id);

    Task<IEnumerable<PlanoTreinamentoResponse>> ListarAsync();
}