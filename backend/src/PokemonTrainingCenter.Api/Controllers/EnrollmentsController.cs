using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Api.Contracts;
using PokemonTrainingCenter.Domain.Persistence;
using PokemonTrainingCenter.Domain.Services;

namespace PokemonTrainingCenter.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly EnrollmentService _enrollmentService;
    private readonly AppDbContext _db;

    public EnrollmentsController(EnrollmentService enrollmentService, AppDbContext db)
    {
        _enrollmentService = enrollmentService;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnrollmentListItemResponse>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? status)
    {
        var enrollments = await _db.Enrollments
            .Include(e => e.Pokemon!)
            .Include(e => e.Trainer)
            .Include(e => e.TrainingPlan)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();

        IEnumerable<Domain.Entities.Enrollment> filtered = enrollments;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = NormalizeForSearch(search);
            // FR-016/FR-027: busca pelo Treinador associado à matrícula
            // (snapshot no momento da criação), não o dono atual do Pokémon.
            filtered = filtered.Where(e =>
                NormalizeForSearch(e.Pokemon!.Name).Contains(normalizedSearch) ||
                NormalizeForSearch(e.Trainer!.Name).Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(e => EnrollmentResponse.ResolveStatus(e) == status);
        }

        return Ok(filtered.Select(EnrollmentListItemResponse.From));
    }

    // FR-016: busca case-insensitive e accent-insensitive (ex.: "agua" encontra "Água").
    // Feita em memória (não traduzida para SQL) para funcionar de forma idêntica
    // em qualquer provider do EF Core, incluindo o InMemory usado nos testes.
    private static string NormalizeForSearch(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
    }

    [HttpPost]
    public async Task<ActionResult<EnrollmentResponse>> Create(CreateEnrollmentRequest request)
    {
        var enrollment = await _enrollmentService.CreateEnrollmentAsync(request.PokemonId, request.TrainingPlanId);
        var response = EnrollmentResponse.From(enrollment);
        return CreatedAtAction(nameof(Create), new { id = enrollment.Id }, response);
    }

    [HttpPost("{id:int}/upgrade/preview")]
    public async Task<ActionResult<UpgradePreviewResponse>> PreviewUpgrade(int id, UpgradeRequest request)
    {
        var result = await _enrollmentService.PreviewUpgradeAsync(id, request.NewTrainingPlanId);
        return Ok(UpgradePreviewResponse.From(result));
    }

    [HttpPost("{id:int}/upgrade/confirm")]
    public async Task<ActionResult<UpgradeConfirmResponse>> ConfirmUpgrade(int id, UpgradeRequest request)
    {
        var (closed, created, proration) = await _enrollmentService.ConfirmUpgradeAsync(id, request.NewTrainingPlanId);
        var response = new UpgradeConfirmResponse(closed.Id, EnrollmentResponse.From(created), proration.FirstChargeAmount);
        return CreatedAtAction(nameof(Create), new { id = created.Id }, response);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<EnrollmentResponse>> Cancel(int id)
    {
        var enrollment = await _enrollmentService.CancelEnrollmentAsync(id);
        return Ok(EnrollmentResponse.From(enrollment));
    }
}
