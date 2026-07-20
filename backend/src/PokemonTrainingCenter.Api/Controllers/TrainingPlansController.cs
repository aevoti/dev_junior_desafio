using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Api.Contracts;
using PokemonTrainingCenter.Domain.Repositories;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/training-plans")]
public class TrainingPlansController : ControllerBase
{
    private readonly ITrainingPlanRepository _trainingPlans;

    public TrainingPlansController(ITrainingPlanRepository trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingPlanResponse>>> GetAll()
    {
        var plans = await _trainingPlans.GetAllOrderedByPriceAsync();

        return Ok(plans.Select(p => new TrainingPlanResponse(p.Id, p.Name, p.MonthlyPrice, p.Description)));
    }
}
