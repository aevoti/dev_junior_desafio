using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Api.Contracts;
using PokemonTrainingCenter.Domain.Persistence;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/training-plans")]
public class TrainingPlansController : ControllerBase
{
    private readonly AppDbContext _db;

    public TrainingPlansController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingPlanResponse>>> GetAll()
    {
        var plans = await _db.TrainingPlans
            .OrderBy(p => p.MonthlyPrice)
            .Select(p => new TrainingPlanResponse(p.Id, p.Name, p.MonthlyPrice, p.Description))
            .ToListAsync();

        return Ok(plans);
    }
}
