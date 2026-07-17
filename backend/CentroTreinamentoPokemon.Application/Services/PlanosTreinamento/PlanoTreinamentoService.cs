using CentroTreinamentoPokemon.DataTransfer.Responses.PlanoTreinamento;
using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Domain.Repositories;

namespace CentroTreinamentoPokemon.Application.Services.PlanosTreinamento;

public class PlanoTreinamentoService : IPlanoTreinamentoService
{
    private readonly IPlanoTreinamentoRepository planoTreinamentoRepository;

    public PlanoTreinamentoService(
        IPlanoTreinamentoRepository planoTreinamentoRepository)
    {
        this.planoTreinamentoRepository = planoTreinamentoRepository;
    }

    public async Task<PlanoTreinamentoResponse?> ObterPorIdAsync(int id)
    {
        PlanoTreinamento? plano =
            await planoTreinamentoRepository.RecuperarPorIdAsync(id);

        if (plano is null)
            return null;

        return MapearResponse(plano);
    }

    public async Task<IEnumerable<PlanoTreinamentoResponse>> ListarAsync()
    {
        IEnumerable<PlanoTreinamento> planos =
            await planoTreinamentoRepository.ListarAsync();

        return planos.Select(MapearResponse);
    }

    private static PlanoTreinamentoResponse MapearResponse(
        PlanoTreinamento plano)
    {
        return new PlanoTreinamentoResponse
        {
            Id = plano.Id,
            Nome = plano.Nome,
            ValorMensal = plano.ValorMensal,
            Descricao = plano.Descricao,
            NivelPlano = plano.NivelPlano
        };
    }
}