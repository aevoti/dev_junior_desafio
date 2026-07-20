namespace PokemonTraining.Api.Entities;

public class PlanoTreinamento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal ValorMensal { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public int NivelMinimo { get; set; }
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
