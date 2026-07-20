using PokemonTrainingCenter.Application.DTOs;
using PokemonTrainingCenter.Application.Interfaces;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Enums;
using PokemonTrainingCenter.Domain.Exceptions;

namespace PokemonTrainingCenter.Application.Services;

public class MatriculaService : IMatriculaService
{
    private const int CicloDias = 30;

    private readonly IMatriculaRepository _matriculaRepository;
    private readonly IPokemonRepository _pokemonRepository;
    private readonly IPlanoTreinamentoRepository _planoRepository;

    public MatriculaService(
        IMatriculaRepository matriculaRepository,
        IPokemonRepository pokemonRepository,
        IPlanoTreinamentoRepository planoRepository)
    {
        _matriculaRepository = matriculaRepository;
        _pokemonRepository = pokemonRepository;
        _planoRepository = planoRepository;
    }

    public async Task<List<MatriculaDto>> ListarAsync(string? nomeBusca, StatusMatricula? status)
    {
        var matriculas = await _matriculaRepository.ListarAsync(nomeBusca, status);
        return matriculas.Select(MapToDto).ToList();
    }

    public async Task<MatriculaDto> CriarAsync(CriarMatriculaRequest request)
    {
        var pokemon = await _pokemonRepository.ObterPorIdAsync(request.PokemonId)
            ?? throw new DomainException("Pokémon não encontrado.");

        var plano = await _planoRepository.ObterPorIdAsync(request.PlanoTreinamentoId)
            ?? throw new DomainException("Plano de treinamento não encontrado.");

        // R1
        var matriculaAtiva = await _matriculaRepository.ObterAtivaPorPokemonAsync(request.PokemonId);
        if (matriculaAtiva is not null)
        {
            throw new DomainException(
                $"{pokemon.Nome} já possui uma matrícula ativa. Cancele-a ou faça upgrade antes de criar uma nova.");
        }

        // R3
        ValidarNivelMinimo(pokemon, plano);

        var matricula = new Matricula
        {
            PokemonId = pokemon.Id,
            PlanoTreinamentoId = plano.Id,
            DataInicio = request.DataInicio.Date,
            Status = StatusMatricula.Ativa,
            ValorMensal = plano.ValorMensal,
        };

        await _matriculaRepository.AdicionarAsync(matricula);

        return new MatriculaDto(
            matricula.Id,
            pokemon.Id,
            pokemon.Nome,
            pokemon.Treinador?.Nome ?? string.Empty,
            plano.Id,
            plano.Nome,
            matricula.DataInicio,
            matricula.DataFim,
            matricula.Status,
            matricula.ValorMensal);
    }

    /// <summary>Calcula o pro-rata do upgrade (R2) sem persistir nada — usado pela UI para
    /// exibir o valor da primeira cobrança antes do Treinador confirmar.</summary>
    public async Task<UpgradeMatriculaResponse> SimularUpgradeAsync(int matriculaId, UpgradeMatriculaRequest request)
    {
        var calculo = await ValidarECalcularUpgradeAsync(matriculaId, request);
        return new UpgradeMatriculaResponse(0, calculo.Credito, calculo.Custo, calculo.Cobranca);
    }

    public async Task<UpgradeMatriculaResponse> UpgradeAsync(int matriculaId, UpgradeMatriculaRequest request)
    {
        var calculo = await ValidarECalcularUpgradeAsync(matriculaId, request);

        calculo.MatriculaAtual.Status = StatusMatricula.Concluida;
        calculo.MatriculaAtual.DataFim = calculo.DataUpgrade;
        await _matriculaRepository.AtualizarAsync(calculo.MatriculaAtual);

        var novaMatricula = new Matricula
        {
            PokemonId = calculo.Pokemon.Id,
            PlanoTreinamentoId = calculo.NovoPlano.Id,
            DataInicio = calculo.DataUpgrade,
            Status = StatusMatricula.Ativa,
            ValorMensal = calculo.NovoPlano.ValorMensal,
            MatriculaOrigemId = calculo.MatriculaAtual.Id,
        };
        await _matriculaRepository.AdicionarAsync(novaMatricula);

        return new UpgradeMatriculaResponse(
            novaMatricula.Id,
            calculo.Credito,
            calculo.Custo,
            calculo.Cobranca);
    }

    private record UpgradeCalculado(
        Matricula MatriculaAtual,
        PlanoTreinamento NovoPlano,
        Pokemon Pokemon,
        DateTime DataUpgrade,
        decimal Credito,
        decimal Custo,
        decimal Cobranca);

    private async Task<UpgradeCalculado> ValidarECalcularUpgradeAsync(int matriculaId, UpgradeMatriculaRequest request)
    {
        var matriculaAtual = await _matriculaRepository.ObterPorIdAsync(matriculaId)
            ?? throw new DomainException("Matrícula não encontrada.");

        if (matriculaAtual.Status != StatusMatricula.Ativa)
        {
            throw new DomainException("Somente matrículas ativas podem receber upgrade.");
        }

        var planoAtual = matriculaAtual.PlanoTreinamento
            ?? throw new DomainException("Plano atual da matrícula não encontrado.");

        var novoPlano = await _planoRepository.ObterPorIdAsync(request.NovoPlanoTreinamentoId)
            ?? throw new DomainException("Novo plano de treinamento não encontrado.");

        if (novoPlano.Id == planoAtual.Id)
        {
            throw new DomainException("O Pokémon já está matriculado neste plano.");
        }

        // R2: downgrade (plano de nível igual ou inferior) não é permitido.
        if (novoPlano.Nivel <= planoAtual.Nivel)
        {
            throw new DomainException("Downgrade não é permitido. Escolha um plano superior ao atual.");
        }

        var pokemon = matriculaAtual.Pokemon
            ?? await _pokemonRepository.ObterPorIdAsync(matriculaAtual.PokemonId)
            ?? throw new DomainException("Pokémon não encontrado.");

        // R3: nível mínimo também vale para upgrade (ex.: upgrade para Elite dos 4).
        ValidarNivelMinimo(pokemon, novoPlano);

        var dataUpgrade = request.DataUpgrade.Date;
        if (dataUpgrade < matriculaAtual.DataInicio.Date)
        {
            throw new DomainException("A data do upgrade não pode ser anterior ao início da matrícula atual.");
        }

        // Ciclo fixo de 30 dias a partir do início da matrícula atual, conforme o
        // exemplo do enunciado. Upgrades solicitados após o fim do ciclo (>30 dias
        // decorridos) não geram crédito nem cobrança proporcional negativa.
        var diasDecorridos = (dataUpgrade - matriculaAtual.DataInicio.Date).Days;
        var diasRestantes = Math.Clamp(CicloDias - diasDecorridos, 0, CicloDias);

        var creditoPlanoAntigo = Math.Round(planoAtual.ValorMensal * diasRestantes / CicloDias, 2);
        var custoNovoPlanoRestante = Math.Round(novoPlano.ValorMensal * diasRestantes / CicloDias, 2);
        var valorPrimeiraCobranca = Math.Max(0, custoNovoPlanoRestante - creditoPlanoAntigo);

        return new UpgradeCalculado(
            matriculaAtual, novoPlano, pokemon, dataUpgrade,
            creditoPlanoAntigo, custoNovoPlanoRestante, valorPrimeiraCobranca);
    }

    public async Task CancelarAsync(int matriculaId)
    {
        var matricula = await _matriculaRepository.ObterPorIdAsync(matriculaId)
            ?? throw new DomainException("Matrícula não encontrada.");

        if (matricula.Status != StatusMatricula.Ativa)
        {
            throw new DomainException("Somente matrículas ativas podem ser canceladas.");
        }

        // R4: matrícula cancelada libera o Pokémon para nova matrícula (índice
        // único de R1 é filtrado por Status = Ativa) e sai do cálculo de MRR.
        matricula.Status = StatusMatricula.Cancelada;
        matricula.DataFim = DateTime.UtcNow.Date;
        await _matriculaRepository.AtualizarAsync(matricula);
    }

    private static void ValidarNivelMinimo(Pokemon pokemon, PlanoTreinamento plano)
    {
        if (pokemon.Nivel < plano.NivelMinimoPokemon)
        {
            throw new DomainException(
                $"{pokemon.Nome} está no nível {pokemon.Nivel}, mas o plano {plano.Nome} exige nível mínimo {plano.NivelMinimoPokemon}.");
        }
    }

    private static MatriculaDto MapToDto(Matricula m) => new(
        m.Id,
        m.PokemonId,
        m.Pokemon?.Nome ?? string.Empty,
        m.Pokemon?.Treinador?.Nome ?? string.Empty,
        m.PlanoTreinamentoId,
        m.PlanoTreinamento?.Nome ?? string.Empty,
        m.DataInicio,
        m.DataFim,
        m.Status,
        m.ValorMensal);
}
