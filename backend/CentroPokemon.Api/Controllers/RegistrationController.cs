using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;

namespace CentroPokemon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase {
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public RegistrationController(IConfiguration configuration) {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status) {
        using var connection = new SqlConnection(_connectionString);

        var sql = @"
            SELECT 
                r.id, 
                r.pokemon_id, 
                p.name AS pokemon_name, 
                t.name AS trainer_name,
                r.training_plan, 
                r.status, 
                r.monthly_value
            FROM Registration r
            INNER JOIN Pokemon p ON r.pokemon_id = p.id
            INNER JOIN Trainer t ON p.trainer_id = t.id
            WHERE 1=1";

        if (!string.IsNullOrEmpty(status)) {
            sql += " AND r.status = @Status";
        }

        if (!string.IsNullOrEmpty(search)) {
            sql += " AND (p.name LIKE @Search OR t.name LIKE @Search)";
        }

        try {
            var registrations = await connection.QueryAsync(sql, new { 
                Status = status, 
                Search = $"%{search}%" 
            });
            return Ok(registrations);
        } catch (Exception e ) {
            return StatusCode(500, new {message = "Erro ao listar matriculas", error = e.Message});
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Registration registration) {

        if (registration.PokemonId is null || registration.TrainingPlan is null) {
            return BadRequest(new {message = "Preencha todos os campos"});
        }

        using var connection = new SqlConnection(_connectionString);

        if (registration.TrainingPlan == "Elite dos 4") {
            var pokemonLevel = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT level FROM Pokemon WHERE id = @PokemonId", new {PokemonId = registration.PokemonId}
            );

            if (pokemonLevel < 50) {
                return BadRequest(new {message = "Apenas Pokemons com nivel >= 50 podem entrar no plano Elite dos 4!"});
            }    
        }

        registration.Status = "Ativa";

        var value = 0;

        if (registration.TrainingPlan == "Ginasio Local") {
            value = 50;
        } else if (registration.TrainingPlan == "Liga Regional") {
            value = 120;
        } else {
            value = 300;
        }
        registration.MonthlyValue = value;

        // verify if pokemon has an active registration
        var verifySql = "SELECT COUNT(1) FROM Registration WHERE pokemon_id = @PokemonId AND status = 'Ativa';";

        int activeRegistrations = await connection.ExecuteScalarAsync<int>(verifySql, new { PokemonId = registration.PokemonId });

        // if is > 0, pokemon has an active registration
        if (activeRegistrations > 0) {
            return BadRequest(new { message = "Este Pokémon já possui uma matrícula ativa! Faça o upgrade ou cancele a matrícula atual." });
        }

        var sql = @"INSERT INTO Registration (pokemon_id, training_plan, init_date, status, monthly_value)
                    VALUES (@PokemonId, @TrainingPlan, @InitDate, @Status, @MonthlyValue);";

        try {
            await connection.ExecuteAsync(sql, new { 
                PokemonId = registration.PokemonId, 
                TrainingPlan = registration.TrainingPlan, 
                InitDate = registration.InitDate,
                Status = registration.Status,
                MonthlyValue = registration.MonthlyValue
            });
            return Ok(new {message = "Matricula realizada com sucesso!"});
        } catch (SqlException e) when (e.Number == 2627 || e.Number == 2601) {
            return BadRequest(new {message = "Pokemon já possui matrícula."});
        } catch (Exception e) {
            return StatusCode(500, new {message = "Erro interno", error = e.Message});
        } 
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) {
        using var connection = new SqlConnection(_connectionString);

        // search registration by id
        var registration = await connection.QueryFirstOrDefaultAsync<Registration>(
            "SELECT id, init_date AS InitDate, monthly_value AS MonthlyValue, status FROM Registration WHERE id = @Id", 
            new { Id = id }
        );

        if (registration == null) {
            return NotFound(new { message = "Matrícula não encontrada." });
        }

        if (registration.Status == "Cancelada") {
            return BadRequest(new { message = "Esta matrícula já está cancelada." });
        }

        if (registration.Status == "Concluida" || registration.Status == "Encerrada") {
            return BadRequest(new { message = $"Não é possível cancelar uma matrícula com status '{registration.Status}'." });
        }

        // calculate usage
        var cancelDate = DateTime.Now.Date;
        var initDate = registration.InitDate.Date;

        int daysUsed = (cancelDate - initDate).Days;
        if (daysUsed <= 0) daysUsed = 1;

        decimal dailyRate = registration.MonthlyValue / 30m;
        decimal finalProRataValue = Math.Round(daysUsed * dailyRate, 2);

        // update status, end_date and value
        var sql = @"UPDATE Registration 
                    SET status = 'Cancelada', 
                        end_date = @CancelDate, 
                        monthly_value = @FinalValue 
                    WHERE id = @Id;";

        try {
            await connection.ExecuteAsync(sql, new { 
                Id = id, 
                CancelDate = cancelDate, 
                FinalValue = finalProRataValue 
            });

            return Ok(new { 
                message = "Matrícula cancelada com sucesso!",
                daysUsed = daysUsed,
                finalValue = finalProRataValue
            });
        } catch (Exception e) {
            return StatusCode(500, new { message = "Erro ao cancelar matrícula", error = e.Message });
        }
    }

    [HttpPost("{id}/upgrade")]
    public async Task<IActionResult> Upgrade(int id, [FromBody] string newPlanInput, [FromQuery] bool commit = false) {
        using var connection = new SqlConnection(_connectionString);

        // select active registration by the id
        var registrationSql = @"SELECT id, pokemon_id, training_plan, init_date, status, monthly_value 
                            FROM Registration WHERE id = @Id AND status = 'Ativa'";
        var currentReg = await connection.QueryFirstOrDefaultAsync<dynamic>(registrationSql, new { Id = id });

        if (currentReg == null) {
            return NotFound(new { message = "Nenhuma matrícula ativa encontrada com este ID." });
        }

        // get the pokemon level
        var pokemonLevel = await connection.QueryFirstOrDefaultAsync<int>(
            "SELECT level FROM Pokemon WHERE id = @PokemonId", new { PokemonId = currentReg.pokemon_id }
        );

        var prices = new Dictionary<string, int> {
            { "Ginasio Local", 50 },
            { "Liga Regional", 120 },
            { "Elite dos 4", 300 }
        };

        string currrentPlan = currentReg.training_plan;
        string newPlan = newPlanInput;

        if (newPlan == "Elite dos 4" && pokemonLevel < 50) {
            return BadRequest(new { message = "Apenas Pokémons com nível >= 50 podem entrar no plano Elite dos 4!" });
        }

        // =============================================
        // CÁLCULO PROPORCIONAL
        // =============================================
        DateTime initDate = currentReg.init_date;
        DateTime todayDate = DateTime.Now;

        int passedDays = (todayDate - initDate).Days;
        if (passedDays < 0) passedDays = 0;
        if (passedDays > 30) passedDays = 30;
        int remainingDays = 30 - passedDays;

        
        decimal oldPlanValue = (decimal)currentReg.monthly_value;
        decimal newPlanValue = prices[newPlan];

        decimal oldPlanCredit = oldPlanValue * ((decimal)remainingDays / 30.0M);
        decimal newPlanRemainingCost = newPlanValue * ((decimal)remainingDays / 30.0M);
        
        decimal firstCharge = newPlanRemainingCost - oldPlanCredit;

        if (!commit) {
            return Ok(new { 
                message = "Simulação realizada com sucesso", 
                valorCobrado = firstCharge 
            });
        }

        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try {
            var sqlClose = "UPDATE Registration SET status = 'Concluida' WHERE id = @Id;";
            await connection.ExecuteAsync(sqlClose, new { Id = currentReg.id }, transaction);

            var sqlInsert = @"INSERT INTO Registration (pokemon_id, training_plan, init_date, status, monthly_value)
                            VALUES (@PokemonId, @TrainingPlan, @InitDate, @Status, @MonthlyValue);";
            
            await connection.ExecuteAsync(sqlInsert, new {
                PokemonId = currentReg.pokemon_id,
                TrainingPlan = newPlan,
                InitDate = todayDate,
                Status = "Ativa",
                MonthlyValue = firstCharge
            }, transaction);

            transaction.Commit();
            return Ok(new { message = $"Upgrade para {newPlan} realizado com sucesso!", valorCobrado = firstCharge });
        } catch (Exception e) {
            transaction.Rollback();
            return StatusCode(500, new { message = "Erro ao processar o upgrade", error = e.Message });
        }
    }

    public class UpgradeRequest {
        public int PokemonId { get; set; }
        public string NewPlan { get; set; } = string.Empty;
    }
}