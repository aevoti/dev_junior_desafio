namespace PokemonTrainingCenter.Domain.Entities;

/// <summary>One of the 3 fixed training plans (Ginásio Local, Liga Regional, Elite dos 4).</summary>
public class TrainingPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
