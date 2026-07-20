namespace CentroPokemon.Api;

public class Pokemon {
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Type {get; set;} = string.Empty;
    public int Level {get; set;}
    public int TrainerId {get; set;}
}