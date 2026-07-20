using PokemonTrainingCenter.Domain.Services;

namespace PokemonTrainingCenter.UnitTests;

public class BillingCycleCalculatorTests
{
    [Fact]
    public void CalculateUpgradeProration_MatchesSpecExample_Day16Of30DayCycle()
    {
        // Abril tem 30 dias: ciclo 01/04 -> 01/05. Upgrade no dia 16 (15 dias restantes).
        var start = new DateTime(2027, 4, 1);
        var upgradeDate = new DateTime(2027, 4, 16);

        var result = BillingCycleCalculator.CalculateUpgradeProration(start, upgradeDate, 50.00m, 120.00m);

        Assert.Equal(15, result.DaysRemainingInCycle);
        Assert.Equal(25.00m, result.CurrentPlanCredit);
        Assert.Equal(60.00m, result.NewPlanProratedCost);
        Assert.Equal(35.00m, result.FirstChargeAmount);
    }

    [Fact]
    public void CalculateUpgradeProration_FirstDayOfCycle_CreditsFullCycle()
    {
        var start = new DateTime(2027, 4, 1);

        var result = BillingCycleCalculator.CalculateUpgradeProration(start, start, 50.00m, 120.00m);

        Assert.Equal(30, result.DaysRemainingInCycle);
        Assert.Equal(50.00m, result.CurrentPlanCredit);
        Assert.Equal(120.00m, result.NewPlanProratedCost);
        Assert.Equal(70.00m, result.FirstChargeAmount);
    }

    [Fact]
    public void CalculateUpgradeProration_LastDayOfCycle_ResultsInMinimalProration()
    {
        // Último dia do ciclo (dias restantes ≈ 0, mas o próprio dia da
        // solicitação ainda conta): ciclo 01/04 -> 01/05, upgrade em 30/04.
        var start = new DateTime(2027, 4, 1);
        var upgradeDate = start.AddMonths(1).AddDays(-1);

        var result = BillingCycleCalculator.CalculateUpgradeProration(start, upgradeDate, 50.00m, 120.00m);

        Assert.Equal(1, result.DaysRemainingInCycle);
        Assert.Equal(1.67m, result.CurrentPlanCredit); // 50 * 1/30 = 1.666... -> 1.67
        Assert.Equal(4.00m, result.NewPlanProratedCost); // 120 * 1/30 = 4.00
        Assert.Equal(2.33m, result.FirstChargeAmount); // 4.00 - 1.67
    }

    [Fact]
    public void CalculateUpgradeProration_RoundsToTwoDecimalPlaces_AwayFromZero()
    {
        // Ciclo de 29 dias (fevereiro não-bissexto): 50 * 15/29 = 25,862... -> 25,86.
        var start = new DateTime(2027, 1, 31);
        var upgradeDate = new DateTime(2027, 2, 14); // 14 dias decorridos de um ciclo de 28 dias (31/01 -> 28/02)

        var result = BillingCycleCalculator.CalculateUpgradeProration(start, upgradeDate, 50.00m, 120.00m);

        // Ciclo real: 31/01 -> 28/02 (não-bissexto) = 28 dias; upgrade em 14/02 = 14 dias decorridos, 14 restantes.
        Assert.Equal(28, (result.CycleEndDate - start).Days);
        Assert.Equal(14, result.DaysRemainingInCycle);
        // 50 * 14/28 = 25.00 exato; validando arredondamento em outro cenário fracionário abaixo.
        Assert.Equal(25.00m, result.CurrentPlanCredit);
    }

    [Fact]
    public void CalculateUpgradeProration_RoundsFractionalResult_HalfAwayFromZero()
    {
        var start = new DateTime(2027, 4, 1); // ciclo de 30 dias
        var upgradeDate = new DateTime(2027, 4, 21); // 9 dias restantes

        var result = BillingCycleCalculator.CalculateUpgradeProration(start, upgradeDate, 50.00m, 120.00m);

        // 50 * 9/30 = 15.00 ; 120 * 9/30 = 36.00 ; ambos exatos neste cenário.
        // Cenário com dízima: ciclo de 29 dias, 15 dias restantes -> 50*15/29 = 25.862... -> 25.86
        var leapStart = new DateTime(2028, 1, 31); // 2028 é bissexto -> fevereiro tem 29 dias
        var leapUpgrade = leapStart.AddDays(14); // 14 dias decorridos, 15 restantes de 29

        var leapResult = BillingCycleCalculator.CalculateUpgradeProration(leapStart, leapUpgrade, 50.00m, 120.00m);

        Assert.Equal(29, (leapResult.CycleEndDate - leapStart).Days);
        Assert.Equal(15, leapResult.DaysRemainingInCycle);
        Assert.Equal(25.86m, leapResult.CurrentPlanCredit); // 50 * 15/29 = 25.8620... -> 25.86
    }

    [Fact]
    public void GetCurrentCycle_ClampsToLastDayOfMonth_WhenStartDayDoesNotExistInNextMonth()
    {
        // Matrícula iniciada em 31/01: ciclo de fevereiro termina no último dia do mês.
        var start = new DateTime(2027, 1, 31); // 2027 não é bissexto
        var (_, cycleEnd) = BillingCycleCalculator.GetCurrentCycle(start, new DateTime(2027, 2, 15));

        Assert.Equal(new DateTime(2027, 2, 28), cycleEnd);
    }

    [Fact]
    public void GetCurrentCycle_ClampsToFeb29_InLeapYear()
    {
        var start = new DateTime(2028, 1, 31); // 2028 é bissexto
        var (_, cycleEnd) = BillingCycleCalculator.GetCurrentCycle(start, new DateTime(2028, 2, 15));

        Assert.Equal(new DateTime(2028, 2, 29), cycleEnd);
    }

    [Fact]
    public void GetCurrentCycle_ReanchorsToOriginalDay_AfterAMonthThatRequiredClamping()
    {
        // O ciclo de março deve voltar a ancorar no dia 31, mesmo depois do
        // ciclo de fevereiro ter sido clampado para o dia 28.
        var start = new DateTime(2027, 1, 31);
        var (cycleStart, cycleEnd) = BillingCycleCalculator.GetCurrentCycle(start, new DateTime(2027, 3, 15));

        Assert.Equal(new DateTime(2027, 2, 28), cycleStart);
        Assert.Equal(new DateTime(2027, 3, 31), cycleEnd);
    }
}
