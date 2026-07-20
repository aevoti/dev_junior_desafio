using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;

namespace CentroPokemon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PokemonController : ControllerBase {
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public PokemonController(IConfiguration configuration) {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() {
        using var connection = new SqlConnection(_connectionString);

        var sql = @"SELECT id, name FROM Pokemon ORDER BY name ASC;";

        try {
            var pokemons = await connection.QueryAsync(sql);
            return Ok(pokemons);
        } catch (Exception e) {
            return StatusCode(500, new {message = "Erro ao listar pokemons", error = e.Message});
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Pokemon pokemon) {

        if (string.IsNullOrWhiteSpace(pokemon.Name) || string.IsNullOrWhiteSpace(pokemon.Type)) {
            return BadRequest(new { message = "Preencha todos os campos" });
        }

        if (pokemon.Level < 1 || pokemon.Level > 100) {
            return BadRequest(new { message = "O nível do Pokémon deve estar entre 1 e 100." });
        }

        if (pokemon.TrainerId <= 0) {
            return BadRequest(new { message = "Selecione um treinador válido" });
        }

        using var connection = new SqlConnection(_connectionString);

        // verify if the trainer really exists in the db
        var trainerExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Trainer WHERE id = @TrainerId", 
            new { TrainerId = pokemon.TrainerId }
        );

        if (trainerExists == 0) {
            return BadRequest(new { message = "O treinador informado não foi encontrado no sistema." });
        }

        var sql = @"INSERT INTO Pokemon (name, type, level, trainer_id)
                    VALUES (@Name, @Type, @Level, @TrainerId)";

        try {
            await connection.ExecuteAsync(sql, pokemon);
            
            return Ok(new { message = "Pokémon cadastrado com sucesso!" });
        } catch (Exception e) {
            return StatusCode(500, new { message = "Erro interno ao cadastrar Pokémon.", error = e.Message });
        } 
    }

    [HttpPatch("{id}/replace-trainer")]
    public async Task<IActionResult> ReplaceTrainer(int id, [FromBody] int newTrainerId) {
        if (newTrainerId <= 0) {
            return BadRequest(new { message = "Selecione um treinador válido." });
        }

        using var connection = new SqlConnection(_connectionString);

        var currentTrainerId = await connection.QueryFirstOrDefaultAsync<int?>(
            "SELECT trainer_id FROM Pokemon WHERE id = @Id", 
            new { Id = id }
        );

        if (currentTrainerId.Value == newTrainerId) {
            return BadRequest(new { message = "O Pokémon já pertence a este treinador." });
        }

        var trainerExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Trainer WHERE id = @TrainerId", 
            new { TrainerId = newTrainerId }
        );

        if (trainerExists == 0) {
            return BadRequest(new { message = "O treinador informado não foi encontrado no sistema." });
        }

        var sql = "UPDATE Pokemon SET trainer_id = @TrainerId WHERE id = @Id;";

        try {
            await connection.ExecuteAsync(sql, new { Id = id, TrainerId = newTrainerId });
            return Ok(new { message = "Pokémon transferido para o novo treinador com sucesso!" });
        } catch (Exception e) {
            return StatusCode(500, new { message = "Erro ao transferir Pokémon.", error = e.Message });
        }
    }
}