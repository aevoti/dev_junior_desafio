using CentroTreinamentoPokemon.DataTransfer.Requests.Matricula;
using CentroTreinamentoPokemon.DataTransfer.Responses.Matricula;
using CentroTreinamentoPokemon.Domain.Entities;
using CentroTreinamentoPokemon.Domain.Enums;
using CentroTreinamentoPokemon.Domain.Exceptions;
using CentroTreinamentoPokemon.Domain.Repositories;

namespace CentroTreinamentoPokemon.Application.Services.Matriculas;

public class MatriculaService : IMatriculaService
{
    private const int DiasCicloMensal = 30;

    private readonly IMatriculaRepository matriculaRepository;
    private readonly IPokemonRepository pokemonRepository;
    private readonly IPlanoTreinamentoRepository planoTreinamentoRepository;
    private readonly IUnitOfWork unitOfWork;

    public MatriculaService(
        IMatriculaRepository matriculaRepository,
        IPokemonRepository pokemonRepository,
        IPlanoTreinamentoRepository planoTreinamentoRepository,
        IUnitOfWork unitOfWork)
    {
        this.matriculaRepository = matriculaRepository;
        this.pokemonRepository = pokemonRepository;
        this.planoTreinamentoRepository = planoTreinamentoRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<MatriculaResponse> CriarAsync(
        MatriculaRequest request)
    {
        Pokemon? pokemon =
            await pokemonRepository.RecuperarPorIdAsync(
                request.PokemonId);

        if (pokemon is null)
        {
            throw new RegraNegocioException(
                "Pokémon não encontrado.");
        }

        PlanoTreinamento? planoTreinamento =
            await planoTreinamentoRepository.RecuperarPorIdAsync(
                request.PlanoTreinamentoId);

        if (planoTreinamento is null)
        {
            throw new RegraNegocioException(
                "Plano de treinamento não encontrado.");
        }

        bool possuiMatriculaAtiva =
            await matriculaRepository
                .ExisteMatriculaAtivaPorPokemonIdAsync(
                    request.PokemonId);

        if (possuiMatriculaAtiva)
        {
            throw new RegraNegocioException(
                "O Pokémon já possui uma matrícula ativa.");
        }

        Matricula matricula = new Matricula(
            pokemon,
            planoTreinamento,
            request.DataInicio);

        await matriculaRepository.InserirAsync(matricula);
        await unitOfWork.CommitAsync();

        return MapearResponse(matricula);
    }

    public async Task<MatriculaResponse?> ObterPorIdAsync(int id)
    {
        Matricula? matricula =
            await matriculaRepository.RecuperarPorIdAsync(id);

        if (matricula is null)
            return null;

        return MapearResponse(matricula);
    }

    public async Task<IEnumerable<MatriculaResponse>> ListarAsync(
        string? busca,
        StatusMatriculaEnum? status)
    {
        IEnumerable<Matricula> matriculas =
            await matriculaRepository.ListarAsync(
                busca,
                status);

        return matriculas.Select(MapearResponse);
    }

    public async Task<MatriculaResponse?> CancelarAsync(
        int id,
        CancelarMatriculaRequest request)
    {
        Matricula? matricula =
            await matriculaRepository.RecuperarPorIdAsync(id);

        if (matricula is null)
            return null;

        matricula.Cancelar(request.DataCancelamento);

        await unitOfWork.CommitAsync();

        return MapearResponse(matricula);
    }

    public async Task<UpgradeMatriculaResponse?> SimularUpgradeAsync(
        int id,
        UpgradeMatriculaRequest request)
    {
        Matricula? matriculaAtual =
            await matriculaRepository.RecuperarPorIdAsync(id);

        if (matriculaAtual is null)
            return null;

        PlanoTreinamento novoPlano =
            await RecuperarEValidarNovoPlanoAsync(
                matriculaAtual,
                request);

        return CalcularUpgrade(
            matriculaAtual,
            novoPlano,
            request.DataUpgrade,
            false,
            null);
    }

    public async Task<UpgradeMatriculaResponse?> ConfirmarUpgradeAsync(
        int id,
        UpgradeMatriculaRequest request)
    {
        Matricula? matriculaAtual =
            await matriculaRepository.RecuperarPorIdAsync(id);

        if (matriculaAtual is null)
            return null;

        PlanoTreinamento novoPlano =
            await RecuperarEValidarNovoPlanoAsync(
                matriculaAtual,
                request);

        UpgradeMatriculaResponse calculo =
            CalcularUpgrade(
                matriculaAtual,
                novoPlano,
                request.DataUpgrade,
                false,
                null);

        matriculaAtual.ConcluirParaUpgrade(
            request.DataUpgrade);

        Matricula novaMatricula = new Matricula(
            matriculaAtual.Pokemon,
            novoPlano,
            request.DataUpgrade);

        await matriculaRepository.InserirAsync(novaMatricula);
        await unitOfWork.CommitAsync();

        calculo.UpgradeConfirmado = true;
        calculo.NovaMatriculaId = novaMatricula.Id;

        return calculo;
    }

    private async Task<PlanoTreinamento>
        RecuperarEValidarNovoPlanoAsync(
            Matricula matriculaAtual,
            UpgradeMatriculaRequest request)
    {
        if (matriculaAtual.Status != StatusMatriculaEnum.Ativa)
        {
            throw new RegraNegocioException(
                "Apenas matrículas ativas podem receber upgrade.");
        }

        if (request.DataUpgrade == default)
        {
            throw new RegraNegocioException(
                "Data do upgrade é obrigatória.");
        }

        if (request.DataUpgrade.Date <
            matriculaAtual.DataInicio.Date)
        {
            throw new RegraNegocioException(
                "A data do upgrade não pode ser anterior à data de início.");
        }

        PlanoTreinamento? novoPlano =
            await planoTreinamentoRepository.RecuperarPorIdAsync(
                request.NovoPlanoTreinamentoId);

        if (novoPlano is null)
        {
            throw new RegraNegocioException(
                "Novo plano de treinamento não encontrado.");
        }

        if (novoPlano.NivelPlano <=
            matriculaAtual.PlanoTreinamento.NivelPlano)
        {
            throw new RegraNegocioException(
                "O novo plano deve ser superior ao plano atual. Downgrade não é permitido.");
        }

        if (novoPlano.NivelPlano == 3 &&
            matriculaAtual.Pokemon.Nivel < 50)
        {
            throw new RegraNegocioException(
                "Apenas Pokémon de nível 50 ou superior podem receber upgrade para o plano Elite dos 4.");
        }

        return novoPlano;
    }

    private static UpgradeMatriculaResponse CalcularUpgrade(
        Matricula matriculaAtual,
        PlanoTreinamento novoPlano,
        DateTime dataUpgrade,
        bool upgradeConfirmado,
        int? novaMatriculaId)
    {
        int diasDecorridos =
            (dataUpgrade.Date -
             matriculaAtual.DataInicio.Date).Days;

        int diasDecorridosNoCiclo =
            diasDecorridos % DiasCicloMensal;

        int diasRestantes =
            DiasCicloMensal - diasDecorridosNoCiclo;

        decimal proporcao =
            (decimal)diasRestantes / DiasCicloMensal;

        decimal creditoPlanoAtual = Math.Round(
            matriculaAtual.ValorMensal * proporcao,
            2);

        decimal custoNovoPlano = Math.Round(
            novoPlano.ValorMensal * proporcao,
            2);

        decimal valorPrimeiraCobranca = Math.Round(
            custoNovoPlano - creditoPlanoAtual,
            2);

        if (valorPrimeiraCobranca < 0)
            valorPrimeiraCobranca = 0;

        return new UpgradeMatriculaResponse
        {
            MatriculaAtualId = matriculaAtual.Id,
            PlanoAtualId =
                matriculaAtual.PlanoTreinamentoId,
            PlanoAtualNome =
                matriculaAtual.PlanoTreinamento.Nome,
            NovoPlanoId = novoPlano.Id,
            NovoPlanoNome = novoPlano.Nome,
            DiasRestantes = diasRestantes,
            CreditoPlanoAtual = creditoPlanoAtual,
            CustoProporcionalNovoPlano = custoNovoPlano,
            ValorPrimeiraCobranca =
                valorPrimeiraCobranca,
            UpgradeConfirmado = upgradeConfirmado,
            NovaMatriculaId = novaMatriculaId
        };
    }

    private static MatriculaResponse MapearResponse(
        Matricula matricula)
    {
        return new MatriculaResponse
        {
            Id = matricula.Id,
            PokemonId = matricula.PokemonId,
            PokemonNome = matricula.Pokemon.Nome,
            TreinadorId = matricula.Pokemon.TreinadorId,
            TreinadorNome =
                matricula.Pokemon.Treinador.Nome,
            PlanoTreinamentoId =
                matricula.PlanoTreinamentoId,
            PlanoTreinamentoNome =
                matricula.PlanoTreinamento.Nome,
            DataInicio = matricula.DataInicio,
            DataEncerramento =
                matricula.DataEncerramento,
            Status = matricula.Status,
            ValorMensal = matricula.ValorMensal
        };
    }
}