using CentroTreinamentoPokemon.Domain.Exceptions;

namespace CentroTreinamentoPokemon.Domain.Entities;

public class Treinador
{
    public virtual int Id { get; protected set; }

    public virtual string Nome { get; protected set; } = null!;

    public virtual string Email { get; protected set; } = null!;

    public virtual string CidadeOrigem { get; protected set; } = null!;

    public virtual IList<Pokemon> Pokemons { get; protected set; }
        = new List<Pokemon>();

    protected Treinador()
    {
    }

    public Treinador(
        string nome,
        string email,
        string cidadeOrigem)
    {
        SetNome(nome);
        SetEmail(email);
        SetCidadeOrigem(cidadeOrigem);
    }

    public virtual void SetNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException(
                "Nome do treinador é obrigatório.");

        if (nome.Trim().Length > 150)
            throw new RegraNegocioException(
                "Nome do treinador deve possuir no máximo 150 caracteres.");

        Nome = nome.Trim();
    }

    public virtual void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new RegraNegocioException(
                "E-mail do treinador é obrigatório.");

        if (!email.Contains("@") || email.Trim().Length > 254)
            throw new RegraNegocioException(
                "E-mail do treinador é inválido.");

        Email = email.Trim().ToLower();
    }

    public virtual void SetCidadeOrigem(string cidadeOrigem)
    {
        if (string.IsNullOrWhiteSpace(cidadeOrigem))
            throw new RegraNegocioException(
                "Cidade de origem é obrigatória.");

        if (cidadeOrigem.Trim().Length > 150)
            throw new RegraNegocioException(
                "Cidade de origem deve possuir no máximo 150 caracteres.");

        CidadeOrigem = cidadeOrigem.Trim();
    }

    public virtual void AddPokemon(Pokemon pokemon)
    {
        if (pokemon is null)
            throw new RegraNegocioException(
                "Pokémon é obrigatório.");

        if (Pokemons.Any(item => item == pokemon))
            throw new RegraNegocioException(
                "Este Pokémon já pertence ao treinador.");

        Pokemons.Add(pokemon);
    }
}