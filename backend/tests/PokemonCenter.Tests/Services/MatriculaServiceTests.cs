using Moq;
using PokemonCenter.Application.DTOs;
using PokemonCenter.Application.Interfaces;
using PokemonCenter.Application.Services;
using PokemonCenter.Domain.Entities;
using PokemonCenter.Domain.Enums;
using PokemonCenter.Domain.Exceptions;
using Xunit;

namespace PokemonCenter.Tests.Services;

public class MatriculaServiceTests
{
    private readonly Mock<IMatriculaRepository> _matriculaRepoMock;
    private readonly Mock<IPokemonRepository> _pokemonRepoMock;
    private readonly Mock<IPlanoRepository> _planoRepoMock;
    private readonly MatriculaService _service;

    public MatriculaServiceTests()
    {
        _matriculaRepoMock = new Mock<IMatriculaRepository>();
        _pokemonRepoMock = new Mock<IPokemonRepository>();
        _planoRepoMock = new Mock<IPlanoRepository>();

        _service = new MatriculaService(
            _matriculaRepoMock.Object,
            _pokemonRepoMock.Object,
            _planoRepoMock.Object);
    }

    private static Plano GinasioLocal() => new() { Id = 1, Nome = "Ginásio Local", ValorMensal = 50m, NivelMinimoRequerido = 1 };
    private static Plano LigaRegional() => new() { Id = 2, Nome = "Liga Regional", ValorMensal = 120m, NivelMinimoRequerido = 1 };
    private static Plano EliteDos4() => new() { Id = 3, Nome = "Elite dos 4", ValorMensal = 300m, NivelMinimoRequerido = 50 };

    private static Pokemon Charizard(int nivel = 55) => new() { Id = 1, Nome = "Charizard", Nivel = nivel, TreinadorId = 1 };


    [Fact]
    public async Task CriarMatricula_DevePermitir_QuandoPokemonNaoTemMatriculaAtiva()
    {
        var pokemon = Charizard();
        var plano = GinasioLocal();
        var request = new CreateMatriculaRequest { PokemonId = 1, PlanoId = 1, DataInicio = DateTime.Today };

        _pokemonRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pokemon);
        _planoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plano);
        _matriculaRepoMock.Setup(r => r.GetMatriculaAtivaByPokemonIdAsync(1)).ReturnsAsync((Matricula?)null);
        _matriculaRepoMock.Setup(r => r.AddAsync(It.IsAny<Matricula>()))
            .ReturnsAsync((Matricula m) => m);

        var resultado = await _service.CriarMatriculaAsync(request);

        Assert.Equal(StatusMatricula.Ativa, resultado.Status);
        _matriculaRepoMock.Verify(r => r.AddAsync(It.IsAny<Matricula>()), Times.Once);
    }

    [Fact]
    public async Task CriarMatricula_DeveRejeitar_QuandoPokemonJaTemMatriculaAtiva()
    {
        var pokemon = Charizard();
        var plano = GinasioLocal();
        var matriculaExistente = new Matricula { Id = 99, PokemonId = 1, Status = StatusMatricula.Ativa };
        var request = new CreateMatriculaRequest { PokemonId = 1, PlanoId = 1, DataInicio = DateTime.Today };

        _pokemonRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pokemon);
        _planoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plano);
        _matriculaRepoMock.Setup(r => r.GetMatriculaAtivaByPokemonIdAsync(1)).ReturnsAsync(matriculaExistente);

        await Assert.ThrowsAsync<DuplicateActiveEnrollmentException>(
            () => _service.CriarMatriculaAsync(request));

        _matriculaRepoMock.Verify(r => r.AddAsync(It.IsAny<Matricula>()), Times.Never);
    }


    [Fact]
    public async Task CriarMatricula_DeveRejeitar_QuandoNivelInsuficienteParaEliteDos4()
    {
        var pokemonNivelBaixo = Charizard(nivel: 30);
        var elite = EliteDos4();
        var request = new CreateMatriculaRequest { PokemonId = 1, PlanoId = 3, DataInicio = DateTime.Today };

        _pokemonRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pokemonNivelBaixo);
        _planoRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(elite);
        _matriculaRepoMock.Setup(r => r.GetMatriculaAtivaByPokemonIdAsync(1)).ReturnsAsync((Matricula?)null);

        await Assert.ThrowsAsync<MinimumLevelNotMetException>(
            () => _service.CriarMatriculaAsync(request));
    }

    [Fact]
    public async Task CriarMatricula_DevePermitir_QuandoNivelExatamente50ParaEliteDos4()
    {
        var pokemonNivel50 = Charizard(nivel: 50);
        var elite = EliteDos4();
        var request = new CreateMatriculaRequest { PokemonId = 1, PlanoId = 3, DataInicio = DateTime.Today };

        _pokemonRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pokemonNivel50);
        _planoRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(elite);
        _matriculaRepoMock.Setup(r => r.GetMatriculaAtivaByPokemonIdAsync(1)).ReturnsAsync((Matricula?)null);
        _matriculaRepoMock.Setup(r => r.AddAsync(It.IsAny<Matricula>())).ReturnsAsync((Matricula m) => m);

        var resultado = await _service.CriarMatriculaAsync(request);

        Assert.Equal(StatusMatricula.Ativa, resultado.Status);
    }


    [Fact]
    public async Task SimularUpgrade_DeveRejeitar_QuandoNovoPlanoForInferior()
    {
        var matriculaAtual = new Matricula
        {
            Id = 1,
            Status = StatusMatricula.Ativa,
            DataInicio = DateTime.Today.AddDays(-10),
            Plano = LigaRegional(),
            Pokemon = Charizard()
        };
        var planoInferior = GinasioLocal();

        _matriculaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(matriculaAtual);
        _planoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(planoInferior);

        await Assert.ThrowsAsync<DowngradeNotAllowedException>(
            () => _service.SimularUpgradeAsync(1, 1, DateTime.Today));
    }

    [Fact]
    public async Task SimularUpgrade_DeveRejeitar_QuandoPlanoForIgual()
    {
        var matriculaAtual = new Matricula
        {
            Id = 1,
            Status = StatusMatricula.Ativa,
            DataInicio = DateTime.Today.AddDays(-10),
            Plano = LigaRegional(),
            Pokemon = Charizard()
        };
        var mesmoPlano = LigaRegional();

        _matriculaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(matriculaAtual);
        _planoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(mesmoPlano);

        await Assert.ThrowsAsync<DowngradeNotAllowedException>(
            () => _service.SimularUpgradeAsync(1, 2, DateTime.Today));
    }


    [Fact]
    public async Task SimularUpgrade_DeveCalcularProRata_ConformeExemploDoEnunciado()
    {
        var dataInicio = new DateTime(2026, 1, 1);
        var dataUpgrade = new DateTime(2026, 1, 16);

        var matriculaAtual = new Matricula
        {
            Id = 1,
            Status = StatusMatricula.Ativa,
            DataInicio = dataInicio,
            Plano = GinasioLocal(),
            Pokemon = Charizard()
        };
        var novoPlano = LigaRegional();

        _matriculaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(matriculaAtual);
        _planoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(novoPlano);

        var resultado = await _service.SimularUpgradeAsync(1, 2, dataUpgrade);

        Assert.Equal(35.00m, resultado.ValorPrimeiraCobranca);
        Assert.Equal(15, resultado.DiasRestantes);
        Assert.Equal(25.00m, resultado.CreditoPlanoAntigo);
        Assert.Equal(60.00m, resultado.CustoNovoPlano);
    }

    [Fact]
    public async Task SimularUpgrade_DeveRejeitar_QuandoNivelInsuficienteParaEliteDos4()
    {
        var matriculaAtual = new Matricula
        {
            Id = 1,
            Status = StatusMatricula.Ativa,
            DataInicio = DateTime.Today.AddDays(-5),
            Plano = LigaRegional(),
            Pokemon = Charizard(nivel: 30)
        };
        var elite = EliteDos4();

        _matriculaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(matriculaAtual);
        _planoRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(elite);

        await Assert.ThrowsAsync<MinimumLevelNotMetException>(
            () => _service.SimularUpgradeAsync(1, 3, DateTime.Today));
    }

    [Fact]
    public async Task SimularUpgrade_DeveRejeitar_QuandoMatriculaNaoEstaAtiva()
    {
        var matriculaCancelada = new Matricula
        {
            Id = 1,
            Status = StatusMatricula.Cancelada,
            DataInicio = DateTime.Today.AddDays(-10),
            Plano = GinasioLocal(),
            Pokemon = Charizard()
        };
        var novoPlano = LigaRegional();

        _matriculaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(matriculaCancelada);
        _planoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(novoPlano);

        await Assert.ThrowsAsync<EnrollmentNotActiveException>(
            () => _service.SimularUpgradeAsync(1, 2, DateTime.Today));
    }

    [Fact]
    public async Task SimularUpgrade_DiasRestantesNuncaNegativo_QuandoUpgradeAposFimDoCiclo()
    {
        var dataInicio = new DateTime(2026, 1, 1);
        var dataUpgrade = new DateTime(2026, 2, 15);

        var matriculaAtual = new Matricula
        {
            Id = 1,
            Status = StatusMatricula.Ativa,
            DataInicio = dataInicio,
            Plano = GinasioLocal(),
            Pokemon = Charizard()
        };
        var novoPlano = LigaRegional();

        _matriculaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(matriculaAtual);
        _planoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(novoPlano);

        var resultado = await _service.SimularUpgradeAsync(1, 2, dataUpgrade);

        Assert.Equal(0, resultado.DiasRestantes);
        Assert.Equal(0m, resultado.ValorPrimeiraCobranca);
    }


    [Fact]
    public async Task ConfirmarUpgrade_DeveEncerrarMatriculaAntiga_ECriarNovaAtiva()
    {
        var dataInicio = new DateTime(2026, 1, 1);
        var dataUpgrade = new DateTime(2026, 1, 16);

        var matriculaAtual = new Matricula
        {
            Id = 1,
            PokemonId = 1,
            Status = StatusMatricula.Ativa,
            DataInicio = dataInicio,
            Plano = GinasioLocal(),
            Pokemon = Charizard()
        };
        var novoPlano = LigaRegional();

        _matriculaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(matriculaAtual);
        _planoRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(novoPlano);
        _matriculaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Matricula>())).Returns(Task.CompletedTask);
        _matriculaRepoMock.Setup(r => r.AddAsync(It.IsAny<Matricula>())).ReturnsAsync((Matricula m) => m);

        var novaMatricula = await _service.ConfirmarUpgradeAsync(1, 2, dataUpgrade);

        Assert.Equal(StatusMatricula.Concluida, matriculaAtual.Status);
        Assert.Equal(StatusMatricula.Ativa, novaMatricula.Status);
        Assert.Equal(2, novaMatricula.PlanoId);
        Assert.Equal(dataUpgrade, novaMatricula.DataInicio);

        _matriculaRepoMock.Verify(r => r.UpdateAsync(matriculaAtual), Times.Once);
        _matriculaRepoMock.Verify(r => r.AddAsync(It.IsAny<Matricula>()), Times.Once);
    }


    [Fact]
    public async Task CancelarMatricula_DeveAlterarStatusParaCancelada()
    {
        var matricula = new Matricula { Id = 1, Status = StatusMatricula.Ativa };

        _matriculaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(matricula);
        _matriculaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Matricula>())).Returns(Task.CompletedTask);

        await _service.CancelarMatriculaAsync(1);

        Assert.Equal(StatusMatricula.Cancelada, matricula.Status);
        _matriculaRepoMock.Verify(r => r.UpdateAsync(matricula), Times.Once);
    }
}