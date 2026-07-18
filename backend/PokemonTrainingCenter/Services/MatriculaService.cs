using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Data;
using PokemonTrainingCenter.DTOs;
using PokemonTrainingCenter.Models;

namespace PokemonTrainingCenter.Services;

public class MatriculaService
{
    private readonly AppDbContext _db;

    public MatriculaService(AppDbContext db) => _db = db;

    public async Task<Matricula> CriarAsync(MatriculaRequest req)
    {
        var pokemon = await _db.Pokemons.FindAsync(req.PokemonId)
            ?? throw new KeyNotFoundException("Pokémon não encontrado.");

        // R3: nível mínimo para Elite dos 4
        if (req.Plano == PlanoTreinamento.EliteDos4 && pokemon.Nivel < 50)
            throw new InvalidOperationException("Apenas Pokémon de nível 50 ou superior podem ser matriculados no plano Elite dos 4.");

        // R1: não pode ter matrícula ativa simultânea
        var jaAtiva = await _db.Matriculas
            .AnyAsync(m => m.PokemonId == req.PokemonId && m.Status == StatusMatricula.Ativa);
        if (jaAtiva)
            throw new InvalidOperationException("Este Pokémon já possui uma matrícula ativa. Cancele a matrícula atual antes de criar uma nova.");

        var matricula = new Matricula
        {
            PokemonId = req.PokemonId,
            Plano = req.Plano,
            DataInicio = DateTime.UtcNow,
            Status = StatusMatricula.Ativa,
            ValorMensal = Matricula.ObterValorPlano(req.Plano)
        };

        _db.Matriculas.Add(matricula);
        await _db.SaveChangesAsync();
        return matricula;
    }

    public UpgradePreviewResponse CalcularUpgrade(Matricula atual, PlanoTreinamento novoPlano)
    {
        // R2: downgrade não permitido
        if ((int)novoPlano <= (int)atual.Plano)
            throw new InvalidOperationException("Downgrade de plano não é permitido. Escolha um plano superior ao atual.");

        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(atual.DataInicio.Year, atual.DataInicio.Month, atual.DataInicio.Day);
        var fimCiclo = inicioMes.AddMonths(1);
        var totalDiasCiclo = (fimCiclo - inicioMes).Days;
        var diasRestantes = Math.Max((fimCiclo - hoje).Days, 0);

        var valorNovoPlano = Matricula.ObterValorPlano(novoPlano);

        // crédito proporcional do plano atual (dias não utilizados)
        var creditoPlanoAntigo = atual.ValorMensal * ((decimal)diasRestantes / totalDiasCiclo);
        // custo do novo plano pelos dias restantes
        var custoNovoPlanoDiasRestantes = valorNovoPlano * ((decimal)diasRestantes / totalDiasCiclo);
        var primeiraCobranca = Math.Max(custoNovoPlanoDiasRestantes - creditoPlanoAntigo, 0);

        return new UpgradePreviewResponse(
            Math.Round(primeiraCobranca, 2),
            Math.Round(creditoPlanoAntigo, 2),
            Math.Round(custoNovoPlanoDiasRestantes, 2),
            diasRestantes,
            $"Upgrade de {atual.Plano} para {novoPlano}: {diasRestantes} dias restantes no ciclo de {totalDiasCiclo} dias."
        );
    }

    public async Task<Matricula> ExecutarUpgradeAsync(int matriculaId, PlanoTreinamento novoPlano)
    {
        var atual = await _db.Matriculas
            .Include(m => m.Pokemon)
            .FirstOrDefaultAsync(m => m.Id == matriculaId && m.Status == StatusMatricula.Ativa)
            ?? throw new KeyNotFoundException("Matrícula ativa não encontrada.");

        // R2: downgrade não permitido
        if ((int)novoPlano <= (int)atual.Plano)
            throw new InvalidOperationException("Downgrade de plano não é permitido.");

        // R3: nível mínimo Elite dos 4
        if (novoPlano == PlanoTreinamento.EliteDos4 && atual.Pokemon.Nivel < 50)
            throw new InvalidOperationException("Apenas Pokémon de nível 50 ou superior podem ser matriculados no plano Elite dos 4.");

        var preview = CalcularUpgrade(atual, novoPlano);

        // encerra matrícula atual
        atual.Status = StatusMatricula.Concluida;

        // cria nova matrícula no plano superior
        var nova = new Matricula
        {
            PokemonId = atual.PokemonId,
            Plano = novoPlano,
            DataInicio = DateTime.UtcNow,
            Status = StatusMatricula.Ativa,
            ValorMensal = preview.PrimeiraCobranca // primeira cobrança proporcional
        };

        _db.Matriculas.Add(nova);
        await _db.SaveChangesAsync();
        return nova;
    }

    public async Task CancelarAsync(int matriculaId)
    {
        var matricula = await _db.Matriculas.FindAsync(matriculaId)
            ?? throw new KeyNotFoundException("Matrícula não encontrada.");

        if (matricula.Status != StatusMatricula.Ativa)
            throw new InvalidOperationException("Apenas matrículas ativas podem ser canceladas.");

        matricula.Status = StatusMatricula.Cancelada;
        await _db.SaveChangesAsync();
    }
}
