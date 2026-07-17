using CentroTreinamentoPokemon.Domain.Enums;
using CentroTreinamentoPokemon.Domain.Exceptions;

namespace CentroTreinamentoPokemon.Domain.Entities;

public class Matricula
{
    public virtual int Id { get; protected set; }

    public virtual int PokemonId { get; protected set; }

    public virtual Pokemon Pokemon { get; protected set; } = null!;

    public virtual int PlanoTreinamentoId { get; protected set; }

    public virtual PlanoTreinamento PlanoTreinamento { get; protected set; }
        = null!;

    public virtual DateTime DataInicio { get; protected set; }

    public virtual DateTime? DataEncerramento { get; protected set; }

    public virtual StatusMatriculaEnum Status { get; protected set; }

    public virtual decimal ValorMensal { get; protected set; }

    protected Matricula()
    {
    }

    public Matricula(
        Pokemon pokemon,
        PlanoTreinamento planoTreinamento,
        DateTime dataInicio)
    {
        SetPokemon(pokemon);
        SetPlanoTreinamento(planoTreinamento);
        SetDataInicio(dataInicio);
        SetStatus(StatusMatriculaEnum.Ativa);
        SetValorMensal(planoTreinamento.ValorMensal);
    }

    public virtual void SetPokemon(Pokemon pokemon)
    {
        if (pokemon is null)
            throw new RegraNegocioException(
                "Pokémon é obrigatório.");

        Pokemon = pokemon;
        PokemonId = pokemon.Id;
    }

    public virtual void SetPlanoTreinamento(
        PlanoTreinamento planoTreinamento)
    {
        if (planoTreinamento is null)
            throw new RegraNegocioException(
                "Plano de treinamento é obrigatório.");

        if (
            planoTreinamento.NivelPlano == 3 &&
            Pokemon.Nivel < 50
        )
        {
            throw new RegraNegocioException(
                "Apenas Pokémon de nível 50 ou superior podem ser matriculados no plano Elite dos 4.");
        }

        PlanoTreinamento = planoTreinamento;
        PlanoTreinamentoId = planoTreinamento.Id;
    }

    public virtual void SetDataInicio(DateTime dataInicio)
    {
        if (dataInicio == default)
            throw new RegraNegocioException(
                "Data de início é obrigatória.");

        DataInicio = dataInicio;
    }

    public virtual void SetStatus(StatusMatriculaEnum status)
    {
        if (!Enum.IsDefined(typeof(StatusMatriculaEnum), status))
            throw new RegraNegocioException(
                "Status da matrícula inválido.");

        Status = status;
    }

    public virtual void SetValorMensal(decimal valorMensal)
    {
        if (valorMensal <= 0)
            throw new RegraNegocioException(
                "Valor mensal deve ser maior que zero.");

        ValorMensal = valorMensal;
    }

    public virtual void Cancelar(DateTime dataCancelamento)
    {
        if (Status != StatusMatriculaEnum.Ativa)
            throw new RegraNegocioException(
                "Apenas matrículas ativas podem ser canceladas.");

        if (dataCancelamento < DataInicio)
            throw new RegraNegocioException(
                "A data de cancelamento não pode ser anterior à data de início.");

        Status = StatusMatriculaEnum.Cancelada;
        DataEncerramento = dataCancelamento;
    }

    public virtual void ConcluirParaUpgrade(DateTime dataUpgrade)
    {
        if (Status != StatusMatriculaEnum.Ativa)
            throw new RegraNegocioException(
                "Apenas matrículas ativas podem receber upgrade.");

        if (dataUpgrade < DataInicio)
            throw new RegraNegocioException(
                "A data de upgrade não pode ser anterior à data de início.");

        Status = StatusMatriculaEnum.Concluida;
        DataEncerramento = dataUpgrade;
    }
}