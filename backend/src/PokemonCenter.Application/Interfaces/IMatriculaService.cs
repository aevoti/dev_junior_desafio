using PokemonCenter.Application.DTOs;
using PokemonCenter.Domain.Entities;

namespace PokemonCenter.Application.Interfaces;

public interface IMatriculaService
{
    Task<Matricula> CriarMatriculaAsync(CreateMatriculaRequest request);
    Task<SimularUpgradeResponse> SimularUpgradeAsync(int matriculaId, int novoPlanoId, DateTime dataUpgrade);
    Task<Matricula> ConfirmarUpgradeAsync(int matriculaId, int novoPlanoId, DateTime dataUpgrade);
    Task CancelarMatriculaAsync(int matriculaId);
}