namespace PokemonCenter.DTOs;

public record PlanoResponse(
        int Id,
        string Nome,
        string Descricao,
        decimal ValorMensal
        );
