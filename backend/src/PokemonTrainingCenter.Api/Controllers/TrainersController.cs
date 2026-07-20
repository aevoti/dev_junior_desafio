using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Api.Contracts;
using PokemonTrainingCenter.Domain.Entities;
using PokemonTrainingCenter.Domain.Persistence;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/trainers")]
public class TrainersController : ControllerBase
{
    // FR-024: exige um domínio com TLD de pelo menos 2 caracteres (ex.: "nome@dominio.com").
    private static readonly Regex EmailFormatRegex = new(
        @"^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$", RegexOptions.Compiled);

    private readonly AppDbContext _db;

    public TrainersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<TrainerResponse>> Create(CreateTrainerRequest request)
    {
        if (!EmailFormatRegex.IsMatch(request.Email))
        {
            return BadRequest(new { message = "E-mail em formato inválido." });
        }

        // FR-002: unicidade de e-mail case-insensitive (collation padrão do SQL Server já é case-insensitive).
        var emailInUse = await _db.Trainers.AnyAsync(t => t.Email == request.Email);
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

        _db.Trainers.Add(trainer);
        await _db.SaveChangesAsync();

        var response = new TrainerResponse(trainer.Id, trainer.Name, trainer.Email, trainer.City);
        return CreatedAtAction(nameof(GetAll), new { id = trainer.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainerResponse>>> GetAll()
    {
        var trainers = await _db.Trainers
            .OrderBy(t => t.Name)
            .Select(t => new TrainerResponse(t.Id, t.Name, t.Email, t.City))
            .ToListAsync();

        return Ok(trainers);
    }
}
