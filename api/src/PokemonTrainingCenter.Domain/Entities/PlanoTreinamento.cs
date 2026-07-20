namespace PokemonTrainingCenter.Domain.Entities;

public class PlanoTreinamento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorMensal { get; set; }

    /// <summary>
    /// Define a ordem hierárquica dos planos (usada para validar upgrade/downgrade - R2).
    /// </summary>
    public int Nivel { get; set; }

    /// <summary>
    /// Nível mínimo do Pokémon exigido para se matricular neste plano (R3: Elite dos 4 = 50).
    /// </summary>
    public int NivelMinimoPokemon { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
