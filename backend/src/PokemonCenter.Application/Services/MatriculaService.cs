using PokemonCenter.Application.DTOs;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Domain.Entities;
using PokemonCenter.Domain.Enums;
using PokemonCenter.Domain.Exceptions;

namespace PokemonCenter.Application.Services;

public class MatriculaService : IMatriculaService
{
    private const int DIAS_NO_CICLO = 30;

    private readonly IMatriculaRepository _matriculaRepository;
    private readonly IPokemonRepository _pokemonRepository;
    private readonly IPlanoRepository _planoRepository;

    public MatriculaService(
        IMatriculaRepository matriculaRepository,
        IPokemonRepository pokemonRepository,
        IPlanoRepository planoRepository)
    {
        _matriculaRepository = matriculaRepository;
        _pokemonRepository = pokemonRepository;
        _planoRepository = planoRepository;
    }

    public async Task<Matricula> CriarMatriculaAsync(CreateMatriculaRequest request)
    {
        var pokemon = await _pokemonRepository.GetByIdAsync(request.PokemonId)
            ?? throw new DomainException("Pokémon não encontrado.");

        var plano = await _planoRepository.GetByIdAsync(request.PlanoId)
            ?? throw new DomainException("Plano não encontrado.");

        var matriculaAtiva = await _matriculaRepository.GetMatriculaAtivaByPokemonIdAsync(request.PokemonId);
        if (matriculaAtiva != null)
            throw new DuplicateActiveEnrollmentException();

        if (pokemon.Nivel < plano.NivelMinimoRequerido)
            throw new MinimumLevelNotMetException();

        var matricula = new Matricula
        {
            PokemonId = request.PokemonId,
            PlanoId = request.PlanoId,
            DataInicio = request.DataInicio,
            Status = StatusMatricula.Ativa,
            ValorMensal = plano.ValorMensal
        };

        return await _matriculaRepository.AddAsync(matricula);
    }

    public async Task<SimularUpgradeResponse> SimularUpgradeAsync(int matriculaId, int novoPlanoId, DateTime dataUpgrade)
    {
        var matriculaAtual = await _matriculaRepository.GetByIdAsync(matriculaId)
            ?? throw new DomainException("Matrícula não encontrada.");

        var novoPlano = await _planoRepository.GetByIdAsync(novoPlanoId)
            ?? throw new DomainException("Plano não encontrado.");

        return CalcularUpgrade(matriculaAtual, novoPlano, dataUpgrade);
    }

    public async Task<Matricula> ConfirmarUpgradeAsync(int matriculaId, int novoPlanoId, DateTime dataUpgrade)
    {
        var matriculaAtual = await _matriculaRepository.GetByIdAsync(matriculaId)
            ?? throw new DomainException("Matrícula não encontrada.");

        var novoPlano = await _planoRepository.GetByIdAsync(novoPlanoId)
            ?? throw new DomainException("Plano não encontrado.");

        CalcularUpgrade(matriculaAtual, novoPlano, dataUpgrade);

        matriculaAtual.Status = StatusMatricula.Concluida;
        await _matriculaRepository.UpdateAsync(matriculaAtual);

        var novaMatricula = new Matricula
        {
            PokemonId = matriculaAtual.PokemonId,
            PlanoId = novoPlano.Id,
            DataInicio = dataUpgrade,
            Status = StatusMatricula.Ativa,
            ValorMensal = novoPlano.ValorMensal
        };

        return await _matriculaRepository.AddAsync(novaMatricula);
    }

    public async Task CancelarMatriculaAsync(int matriculaId)
    {
        var matricula = await _matriculaRepository.GetByIdAsync(matriculaId)
            ?? throw new DomainException("Matrícula não encontrada.");

        matricula.Status = StatusMatricula.Cancelada;
        await _matriculaRepository.UpdateAsync(matricula);
    }

    private SimularUpgradeResponse CalcularUpgrade(Matricula matriculaAtual, Plano novoPlano, DateTime dataUpgrade)
    {
        if (matriculaAtual.Status != StatusMatricula.Ativa)
            throw new EnrollmentNotActiveException();

        if (novoPlano.ValorMensal <= matriculaAtual.Plano!.ValorMensal)
            throw new DowngradeNotAllowedException();

        if (matriculaAtual.Pokemon!.Nivel < novoPlano.NivelMinimoRequerido)
            throw new MinimumLevelNotMetException();

        int diasUtilizados = (dataUpgrade.Date - matriculaAtual.DataInicio.Date).Days;
        diasUtilizados = Math.Clamp(diasUtilizados, 0, DIAS_NO_CICLO);

        int diasRestantes = DIAS_NO_CICLO - diasUtilizados;
        decimal proporcaoRestante = (decimal)diasRestantes / DIAS_NO_CICLO;

        decimal creditoPlanoAntigo = matriculaAtual.Plano.ValorMensal * proporcaoRestante;
        decimal custoNovoPlano = novoPlano.ValorMensal * proporcaoRestante;
        decimal primeiraCobranca = Math.Round(custoNovoPlano - creditoPlanoAntigo, 2);

        return new SimularUpgradeResponse
        {
            ValorPrimeiraCobranca = primeiraCobranca,
            DiasRestantes = diasRestantes,
            CreditoPlanoAntigo = Math.Round(creditoPlanoAntigo, 2),
            CustoNovoPlano = Math.Round(custoNovoPlano, 2)
        };
    }
}