using CentroTreinamentoPokemon.Domain.Enums;
using CentroTreinamentoPokemon.Domain.Exceptions;

namespace CentroTreinamentoPokemon.Domain.Entities;

public class Pokemon
{
    public virtual int Id { get; protected set; }

    public virtual string Nome { get; protected set; } = null!;

    public virtual TipoPokemonEnum Tipo { get; protected set; }

    public virtual int Nivel { get; protected set; }

    public virtual int TreinadorId { get; protected set; }

    public virtual Treinador Treinador { get; protected set; } = null!;

    public virtual IList<Matricula> Matriculas { get; protected set; }
        = new List<Matricula>();

    protected Pokemon()
    {
    }

    public Pokemon(
        string nome,
        TipoPokemonEnum tipo,
        int nivel,
        Treinador treinador)
    {
        SetNome(nome);
        SetTipo(tipo);
        SetNivel(nivel);
        SetTreinador(treinador);
    }

    public virtual void SetNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException(
                "Nome do Pokémon é obrigatório.");

        if (nome.Trim().Length > 100)
            throw new RegraNegocioException(
                "Nome do Pokémon deve possuir no máximo 100 caracteres.");

        Nome = nome.Trim();
    }

    public virtual void SetTipo(TipoPokemonEnum tipo)
    {
        if (!Enum.IsDefined(typeof(TipoPokemonEnum), tipo))
            throw new RegraNegocioException(
                "Tipo do Pokémon inválido.");

        Tipo = tipo;
    }

    public virtual void SetNivel(int nivel)
    {
        if (nivel < 1 || nivel > 100)
            throw new RegraNegocioException(
                "O nível do Pokémon deve estar entre 1 e 100.");

        Nivel = nivel;
    }

    public virtual void SetTreinador(Treinador treinador)
    {
        if (treinador is null)
            throw new RegraNegocioException(
                "Treinador é obrigatório.");

        Treinador = treinador;
        TreinadorId = treinador.Id;
    }

    public virtual void AddMatricula(Matricula matricula)
    {
        if (matricula is null)
            throw new RegraNegocioException(
                "Matrícula é obrigatória.");

        Matriculas.Add(matricula);
    }

    public virtual void TransferirTreinador(Treinador treinador)
    {
        if (treinador is null)
            throw new RegraNegocioException(
                "O novo treinador é obrigatório.");

        if (treinador.Id == TreinadorId)
            throw new RegraNegocioException(
                "O Pokémon já pertence a este treinador.");

        SetTreinador(treinador);
    }
}