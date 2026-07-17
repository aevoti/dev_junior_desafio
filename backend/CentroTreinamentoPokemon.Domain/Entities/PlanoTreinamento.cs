using CentroTreinamentoPokemon.Domain.Exceptions;

namespace CentroTreinamentoPokemon.Domain.Entities;

public class PlanoTreinamento
{
    public virtual int Id { get; protected set; }

    public virtual string Nome { get; protected set; } = null!;

    public virtual decimal ValorMensal { get; protected set; }

    public virtual string Descricao { get; protected set; } = null!;

    public virtual int NivelPlano { get; protected set; }

    public virtual IList<Matricula> Matriculas { get; protected set; }
        = new List<Matricula>();

    protected PlanoTreinamento()
    {
    }

    public PlanoTreinamento(
        string nome,
        decimal valorMensal,
        string descricao,
        int nivelPlano)
    {
        SetNome(nome);
        SetValorMensal(valorMensal);
        SetDescricao(descricao);
        SetNivelPlano(nivelPlano);
    }

    public virtual void SetNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException(
                "Nome do plano é obrigatório.");

        if (nome.Trim().Length > 100)
            throw new RegraNegocioException(
                "Nome do plano deve possuir no máximo 100 caracteres.");

        Nome = nome.Trim();
    }

    public virtual void SetValorMensal(decimal valorMensal)
    {
        if (valorMensal <= 0)
            throw new RegraNegocioException(
                "Valor mensal do plano deve ser maior que zero.");

        ValorMensal = valorMensal;
    }

    public virtual void SetDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new RegraNegocioException(
                "Descrição do plano é obrigatória.");

        if (descricao.Trim().Length > 300)
            throw new RegraNegocioException(
                "Descrição do plano deve possuir no máximo 300 caracteres.");

        Descricao = descricao.Trim();
    }

    public virtual void SetNivelPlano(int nivelPlano)
    {
        if (nivelPlano < 1 || nivelPlano > 3)
            throw new RegraNegocioException(
                "O nível do plano deve estar entre 1 e 3.");

        NivelPlano = nivelPlano;
    }
}