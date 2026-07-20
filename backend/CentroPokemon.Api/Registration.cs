namespace CentroPokemon.Api;

public class Registration {
    public int Id {get; set;}
    public int? PokemonId {get; set;}
    public string TrainingPlan {get; set;} = string.Empty;
    public DateTime InitDate {get; set;}
    public string Status {get; set;} = string.Empty;
    public decimal MonthlyValue {get; set;}
}