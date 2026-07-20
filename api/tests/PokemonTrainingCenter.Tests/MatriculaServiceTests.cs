using PokemonTrainingCenter.Application.DTOs;
using PokemonTrainingCenter.Application.Services;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Enums;
using PokemonTrainingCenter.Domain.Exceptions;
using PokemonTrainingCenter.Tests.Fakes;
using Xunit;

namespace PokemonTrainingCenter.Tests;

public class MatriculaServiceTests
{
    private readonly FakePokemonRepository _pokemonRepository = new();
    private readonly FakePlanoTreinamentoRepository _planoRepository = new();
    private readonly FakeMatriculaRepository _matriculaRepository;
    private readonly MatriculaService _service;

    private readonly PlanoTreinamento _ginasioLocal = new() { Id = 1, Nome = "Ginásio Local", ValorMensal = 50m, Nivel = 1, NivelMinimoPokemon = 1 };
    private readonly PlanoTreinamento _ligaRegional = new() { Id = 2, Nome = "Liga Regional", ValorMensal = 120m, Nivel = 2, NivelMinimoPokemon = 1 };
    private readonly PlanoTreinamento _eliteDos4 = new() { Id = 3, Nome = "Elite dos 4", ValorMensal = 300m, Nivel = 3, NivelMinimoPokemon = 50 };

    private readonly Pokemon _charmander = new() { Id = 1, Nome = "Charmander", Nivel = 30, TreinadorId = 1 };

    public MatriculaServiceTests()
    {
        _planoRepository.Dados.AddRange(new[] { _ginasioLocal, _ligaRegional, _eliteDos4 });
        _pokemonRepository.Dados.Add(_charmander);
        _matriculaRepository = new FakeMatriculaRepository(_planoRepository.Dados, _pokemonRepository.Dados);
        _service = new MatriculaService(_matriculaRepository, _pokemonRepository, _planoRepository);
    }

    // ------------------------------------------------------------------
    // R1 — matrícula única ativa
    // ------------------------------------------------------------------

    [Fact]
    public async Task CriarAsync_Sucesso_CriaMatriculaAtiva()
    {
        var dto = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 7, 1)));

        Assert.Equal(StatusMatricula.Ativa, dto.Status);
        Assert.Equal(50m, dto.ValorMensal);
        Assert.Single(_matriculaRepository.Dados);
    }

    [Fact]
    public async Task CriarAsync_RejeitaSeJaTemMatriculaAtiva_R1()
    {
        await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 7, 1)));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ligaRegional.Id, new DateTime(2026, 7, 1))));

        Assert.Contains("já possui uma matrícula ativa", ex.Message);
    }

    [Fact]
    public async Task CriarAsync_PermiteNovaMatriculaAposCancelamento_R1_R4()
    {
        var criada = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 7, 1)));
        await _service.CancelarAsync(criada.Id);

        var nova = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ligaRegional.Id, new DateTime(2026, 7, 2)));

        Assert.Equal(StatusMatricula.Ativa, nova.Status);
    }

    // ------------------------------------------------------------------
    // R3 — nível mínimo para a Elite dos 4
    // ------------------------------------------------------------------

    [Fact]
    public async Task CriarAsync_RejeitaNivelInsuficienteParaEliteDos4_R3()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _eliteDos4.Id, new DateTime(2026, 7, 1))));

        Assert.Contains("nível mínimo", ex.Message);
    }

    [Fact]
    public async Task CriarAsync_AceitaNivelExatamenteNoMinimo_R3_CasoDeBorda()
    {
        _charmander.Nivel = 50;

        var dto = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _eliteDos4.Id, new DateTime(2026, 7, 1)));

        Assert.Equal(StatusMatricula.Ativa, dto.Status);
    }

    // ------------------------------------------------------------------
    // R2 — upgrade com pro-rata
    // ------------------------------------------------------------------

    [Fact]
    public async Task UpgradeAsync_CalculaProRataConformeExemploDoEnunciado_R2()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 1, 1)));

        var resultado = await _service.UpgradeAsync(matricula.Id,
            new UpgradeMatriculaRequest(_ligaRegional.Id, new DateTime(2026, 1, 16)));

        Assert.Equal(25.00m, resultado.CreditoPlanoAntigo);
        Assert.Equal(60.00m, resultado.CustoNovoPlanoRestante);
        Assert.Equal(35.00m, resultado.ValorPrimeiraCobranca);
    }

    [Fact]
    public async Task UpgradeAsync_NoPrimeiroDiaDoCiclo_CreditaOMesTodo_CasoDeBorda()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 1, 1)));

        var resultado = await _service.UpgradeAsync(matricula.Id,
            new UpgradeMatriculaRequest(_ligaRegional.Id, new DateTime(2026, 1, 1)));

        Assert.Equal(50.00m, resultado.CreditoPlanoAntigo);
        Assert.Equal(120.00m, resultado.CustoNovoPlanoRestante);
        Assert.Equal(70.00m, resultado.ValorPrimeiraCobranca);
    }

    [Fact]
    public async Task UpgradeAsync_NoUltimoDiaDoCiclo_NaoGeraCreditoNemCobranca_CasoDeBorda()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 1, 1)));

        var resultado = await _service.UpgradeAsync(matricula.Id,
            new UpgradeMatriculaRequest(_ligaRegional.Id, new DateTime(2026, 1, 31)));

        Assert.Equal(0m, resultado.CreditoPlanoAntigo);
        Assert.Equal(0m, resultado.CustoNovoPlanoRestante);
        Assert.Equal(0m, resultado.ValorPrimeiraCobranca);
    }

    [Fact]
    public async Task UpgradeAsync_RejeitaDowngrade_R2()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ligaRegional.Id, new DateTime(2026, 1, 1)));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpgradeAsync(matricula.Id, new UpgradeMatriculaRequest(_ginasioLocal.Id, new DateTime(2026, 1, 10))));

        Assert.Contains("Downgrade não é permitido", ex.Message);
    }

    [Fact]
    public async Task UpgradeAsync_RejeitaMesmoPlano_R2()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ligaRegional.Id, new DateTime(2026, 1, 1)));

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpgradeAsync(matricula.Id, new UpgradeMatriculaRequest(_ligaRegional.Id, new DateTime(2026, 1, 10))));
    }

    [Fact]
    public async Task UpgradeAsync_RejeitaSeNivelInsuficienteParaNovoPlano_R2_R3()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ligaRegional.Id, new DateTime(2026, 1, 1)));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpgradeAsync(matricula.Id, new UpgradeMatriculaRequest(_eliteDos4.Id, new DateTime(2026, 1, 10))));

        Assert.Contains("nível mínimo", ex.Message);
    }

    [Fact]
    public async Task UpgradeAsync_EncerraMatriculaAntigaECriaNovaLigadaPorMatriculaOrigemId_R2()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 1, 1)));

        var resultado = await _service.UpgradeAsync(matricula.Id,
            new UpgradeMatriculaRequest(_ligaRegional.Id, new DateTime(2026, 1, 16)));

        var antiga = _matriculaRepository.Dados.Single(m => m.Id == matricula.Id);
        var nova = _matriculaRepository.Dados.Single(m => m.Id == resultado.NovaMatriculaId);

        Assert.Equal(StatusMatricula.Concluida, antiga.Status);
        Assert.Equal(StatusMatricula.Ativa, nova.Status);
        Assert.Equal(antiga.Id, nova.MatriculaOrigemId);
    }

    [Fact]
    public async Task SimularUpgradeAsync_NaoPersisteNada()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 1, 1)));
        var totalAntes = _matriculaRepository.Dados.Count;

        var resultado = await _service.SimularUpgradeAsync(matricula.Id,
            new UpgradeMatriculaRequest(_ligaRegional.Id, new DateTime(2026, 1, 16)));

        Assert.Equal(35.00m, resultado.ValorPrimeiraCobranca);
        Assert.Equal(0, resultado.NovaMatriculaId);
        Assert.Equal(totalAntes, _matriculaRepository.Dados.Count);
        Assert.Equal(StatusMatricula.Ativa, _matriculaRepository.Dados.Single(m => m.Id == matricula.Id).Status);
    }

    // ------------------------------------------------------------------
    // R4 — matrículas canceladas
    // ------------------------------------------------------------------

    [Fact]
    public async Task CancelarAsync_MarcaComoCanceladaELiberaOPokemon_R4()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 1, 1)));

        await _service.CancelarAsync(matricula.Id);

        var cancelada = _matriculaRepository.Dados.Single(m => m.Id == matricula.Id);
        Assert.Equal(StatusMatricula.Cancelada, cancelada.Status);
        Assert.NotNull(cancelada.DataFim);
    }

    [Fact]
    public async Task CancelarAsync_RejeitaSeNaoEstiverAtiva_R4()
    {
        var matricula = await _service.CriarAsync(new CriarMatriculaRequest(_charmander.Id, _ginasioLocal.Id, new DateTime(2026, 1, 1)));
        await _service.CancelarAsync(matricula.Id);

        await Assert.ThrowsAsync<DomainException>(() => _service.CancelarAsync(matricula.Id));
    }
}
