using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Net.Mail;

namespace CentroPokemon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainerController : ControllerBase {
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public TrainerController(IConfiguration configuration) {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() {
        using var connection = new SqlConnection(_connectionString);

        var sql = "SELECT id, name FROM Trainer ORDER BY name ASC;";

        try {
            var trainers = await connection.QueryAsync(sql);
            return Ok(trainers);
        } catch (Exception e ) {
            return StatusCode(500, new {message = "Erro ao listar treinadores", error = e.Message});
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Trainer trainer) {
        // empty fields
        if (string.IsNullOrWhiteSpace(trainer.Name) || string.IsNullOrWhiteSpace(trainer.Email) || string.IsNullOrWhiteSpace(trainer.OriginCity)) {
            return BadRequest(new { message = "Preencha todos os campos" });
        }

        // email format
        try {
            var mailAddress = new MailAddress(trainer.Email);
        } catch (FormatException) {
            return BadRequest(new { message = "Email inválido! Insira no formato exemplo@gmail.com" });
        }
        
        using var connection = new SqlConnection(_connectionString);

        var sql = @"INSERT INTO Trainer (name, email, origin_city)
                    VALUES (@Name, @Email, @OriginCity);";

        try {
            await connection.ExecuteAsync(sql, trainer);
            return Ok(new { message = "Treinador cadastrado com sucesso!" });
        } catch (SqlException e) when (e.Number == 2627 || e.Number == 2601) {
            return BadRequest(new { message = "Email já cadastrado." });
        } catch (Exception e) {
            return StatusCode(500, new { message = "Erro interno", error = e.Message });
        } 
    }
}