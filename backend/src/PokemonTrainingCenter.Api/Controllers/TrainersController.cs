using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PokemonTrainingCenter.Api.Contracts;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Repositories;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/trainers")]
public class TrainersController : ControllerBase
{
    // FR-024: exige um domínio com TLD de pelo menos 2 caracteres (ex.: "nome@dominio.com").
    private static readonly Regex EmailFormatRegex = new(
        @"^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$", RegexOptions.Compiled);

    private readonly ITrainerRepository _trainers;
    private readonly IUnitOfWork _unitOfWork;

    public TrainersController(ITrainerRepository trainers, IUnitOfWork unitOfWork)
    {
        _trainers = trainers;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<ActionResult<TrainerResponse>> Create(CreateTrainerRequest request)
    {
        if (!EmailFormatRegex.IsMatch(request.Email))
        {
            return BadRequest(new { message = "E-mail em formato inválido." });
        }

        // FR-002: unicidade de e-mail case-insensitive (collation padrão do SQL Server já é case-insensitive).
        var emailInUse = await _trainers.ExistsByEmailAsync(request.Email);
        if (emailInUse)
        {
            return Conflict(new { message = "Este e-mail já está cadastrado para outro Treinador." });
        }

        var trainer = new Trainer
        {
            Name = request.Name,
            Email = request.Email,
            City = request.City
        };

        _trainers.Add(trainer);
        await _unitOfWork.SaveChangesAsync();

        var response = new TrainerResponse(trainer.Id, trainer.Name, trainer.Email, trainer.City);
        return CreatedAtAction(nameof(GetAll), new { id = trainer.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainerResponse>>> GetAll()
    {
        var trainers = await _trainers.GetAllOrderedByNameAsync();

        return Ok(trainers.Select(t => new TrainerResponse(t.Id, t.Name, t.Email, t.City)));
    }
}
