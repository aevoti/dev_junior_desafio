using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonTrainingCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentTrainerSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrainerId",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // FR-027: backfill das matrículas existentes a partir do dono
            // ATUAL do Pokémon — a única informação disponível para dados
            // criados antes deste snapshot existir (ver plan.md, "parte 2").
            // Matrículas criadas a partir de agora recebem o valor correto
            // diretamente em EnrollmentService.
            migrationBuilder.Sql(@"
                UPDATE e
                SET e.TrainerId = p.TrainerId
                FROM [Enrollments] e
                INNER JOIN [Pokemons] p ON p.Id = e.PokemonId;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TrainerId",
                table: "Enrollments",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Trainers_TrainerId",
                table: "Enrollments",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Trainers_TrainerId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_TrainerId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "TrainerId",
                table: "Enrollments");
        }
    }
}
