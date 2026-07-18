using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonTrainingCenter.Data;
using PokemonTrainingCenter.DTOs;
using PokemonTrainingCenter.Models;
using PokemonTrainingCenter.Services;

namespace PokemonTrainingCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatriculasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MatriculaService _service;

    public MatriculasController(AppDbContext db, MatriculaService service)
    {
        _db = db;
        _service = service;
    }

    [HttpGet]
    public async Task<IEnumerable<MatriculaResponse>> GetAll(
        [FromQuery] string? busca,
        [FromQuery] string? status)
    {
        var query = _db.Matriculas
            .Include(m => m.Pokemon).ThenInclude(p => p.Treinador)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var b = busca.ToLower();
            query = query.Where(m =>
                m.Pokemon.Nome.ToLower().Contains(b) ||
                m.Pokemon.Treinador.Nome.ToLower().Contains(b));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StatusMatricula>(status, out var statusEnum))
            query = query.Where(m => m.Status == statusEnum);

        return await query
            .Select(m => new MatriculaResponse(
                m.Id,
                m.PokemonId,
                m.Pokemon.Nome,
                m.Pokemon.Treinador.Nome,
                m.Plano.ToString(),
                m.DataInicio,
                m.Status.ToString(),
                m.ValorMensal))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatriculaResponse>> GetById(int id)
    {
        var m = await _db.Matriculas
            .Include(m => m.Pokemon).ThenInclude(p => p.Treinador)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (m is null) return NotFound();

        return new MatriculaResponse(
            m.Id, m.PokemonId, m.Pokemon.Nome, m.Pokemon.Treinador.Nome,
            m.Plano.ToString(), m.DataInicio, m.Status.ToString(), m.ValorMensal);
    }

    [HttpPost]
    public async Task<ActionResult<MatriculaResponse>> Create([FromBody] MatriculaRequest req)
    {
        try
        {
            var matricula = await _service.CriarAsync(req);
            await _db.Entry(matricula).Reference(m => m.Pokemon).LoadAsync();
            await _db.Entry(matricula.Pokemon).Reference(p => p.Treinador).LoadAsync();

            var resp = new MatriculaResponse(
                matricula.Id, matricula.PokemonId,
                matricula.Pokemon.Nome, matricula.Pokemon.Treinador.Nome,
                matricula.Plano.ToString(), matricula.DataInicio,
                matricula.Status.ToString(), matricula.ValorMensal);

            return CreatedAtAction(nameof(GetById), new { id = matricula.Id }, resp);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { erro = ex.Message }); }
        catch (InvalidOperationException ex) { return UnprocessableEntity(new { erro = ex.Message }); }
    }

    // Prévia do upgrade com valor da primeira cobrança (R2)
    [HttpGet("{id}/upgrade/preview")]
    public async Task<ActionResult<UpgradePreviewResponse>> PreviewUpgrade(int id, [FromQuery] PlanoTreinamento novoPlano)
    {
        var matricula = await _db.Matriculas
            .FirstOrDefaultAsync(m => m.Id == id && m.Status == StatusMatricula.Ativa);

        if (matricula is null)
            return NotFound(new { erro = "Matrícula ativa não encontrada." });

        try
        {
            return _service.CalcularUpgrade(matricula, novoPlano);
        }
        catch (InvalidOperationException ex) { return UnprocessableEntity(new { erro = ex.Message }); }
    }

    // Executa o upgrade (R2)
    [HttpPost("{id}/upgrade")]
    public async Task<ActionResult<MatriculaResponse>> ExecutarUpgrade(int id, [FromBody] UpgradeRequest req)
    {
        try
        {
            var nova = await _service.ExecutarUpgradeAsync(id, req.NovoPlano);
            await _db.Entry(nova).Reference(m => m.Pokemon).LoadAsync();
            await _db.Entry(nova.Pokemon).Reference(p => p.Treinador).LoadAsync();

            var resp = new MatriculaResponse(
                nova.Id, nova.PokemonId,
                nova.Pokemon.Nome, nova.Pokemon.Treinador.Nome,
                nova.Plano.ToString(), nova.DataInicio,
                nova.Status.ToString(), nova.ValorMensal);

            return CreatedAtAction(nameof(GetById), new { id = nova.Id }, resp);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { erro = ex.Message }); }
        catch (InvalidOperationException ex) { return UnprocessableEntity(new { erro = ex.Message }); }
    }

    // Cancelar matrícula (R4)
    [HttpPatch("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        try
        {
            await _service.CancelarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { erro = ex.Message }); }
        catch (InvalidOperationException ex) { return UnprocessableEntity(new { erro = ex.Message }); }
    }
}
